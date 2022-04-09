import {
  CancellationToken,
  DocumentSemanticTokensProvider,
  languages,
  Position,
  Range,
  SemanticTokensBuilder,
  SemanticTokensLegend,
  TextDocument,
} from "vscode";
import { IStackTree } from "./StackDocumentProvider";
import * as SemanticFilters from './SemanticFilters';

export interface IDocumentLine {
  fullLine: string;
  lineNumber: number;
  remainingLine: string;
  currentIndex: number;
}

export enum StackType {
  simplified,
  full
}

const buildRange = (line: number, startingPoint: number, endingPoint: number) => new Range(new Position(line, startingPoint), new Position(line, endingPoint));

const runSemanticFilters = (semanticFilters: (() => void)[]) => semanticFilters.forEach((filter) => filter());

const semanticStackBuilder = (docLine: IDocumentLine, json: IStackTree[], stackType: StackType, tokensBuilder: SemanticTokensBuilder) => {
  const { lineNumber, fullLine } = docLine;
  const jsonLine = json[lineNumber];
  if (jsonLine.typeDiscriminator === 1) {
    tokensBuilder.push(
      buildRange(lineNumber, 0, fullLine.length),
      'comment',
      ['definition']
    );
    return;
  }

  const { fullClass, method, assemblySignature } = jsonLine.typeValue;
  const filters: (() => void)[] = [];

  if (stackType === StackType.full) {
    // Assembly signature
    // Full class name sections
    // Full class generic paramater types
    // Method name
    // Method generic param types
    // Method params
    filters.push(...[
      () => SemanticFilters.assemblySignature(assemblySignature, docLine, tokensBuilder),
      () => fullClass && SemanticFilters.classNameSections(fullClass.nameSections, docLine, tokensBuilder),
      () => fullClass && SemanticFilters.rawGenericParameterType(fullClass.rawGenericParameterTypes, docLine, tokensBuilder),
      () => SemanticFilters.methodName(method, docLine, tokensBuilder),
      () => SemanticFilters.rawGenericParameterType(method.rawGenericParameterTypes, docLine, tokensBuilder),
      () => SemanticFilters.methodParameterType(method, docLine, tokensBuilder, true)
    ]);
  } else if (stackType === StackType.simplified) {
    // Short class name
    // Full class generic paramater types
    // Method name
    // Method generic param types
    // Method params
    filters.push(...[
      () => fullClass && SemanticFilters.classNameFilter(fullClass.shortClassNameOrDefault, docLine, tokensBuilder),
      () => fullClass && SemanticFilters.genericParamaterTypeBuilder(fullClass.genericParameterTypes, docLine, tokensBuilder),
      () => SemanticFilters.methodName(method, docLine, tokensBuilder),
      () => SemanticFilters.genericParamaterTypeBuilder(method.genericParameterTypes, docLine, tokensBuilder),
      () => SemanticFilters.methodParameterType(method, docLine, tokensBuilder)
    ]);
  }

  runSemanticFilters(filters);
};

export const generateColorizer = (json: IStackTree[], fileUri: string, stackType: StackType = StackType.simplified) => {
  const tokenTypes = ['class', 'parameter', 'function', 'interface', 'label', 'typeParameter', 'comment'];
  const tokenModifiers = ['declaration', 'documentation', 'definition', 'defaultLibrary'];
  const legend = new SemanticTokensLegend(tokenTypes, tokenModifiers);

  // Maybe move this into where we parse the json to begin with. Only problem is it feels like we'd be abandoning the spirit of the semantic tokenizer
  const provider: DocumentSemanticTokensProvider = {
    provideDocumentSemanticTokens(document: TextDocument, _token: CancellationToken) {
      const tokensBuilder = new SemanticTokensBuilder(legend);
      document.getText().split('\n').forEach((line, i) => {
        const docLine: IDocumentLine = {
          fullLine: line,
          lineNumber: i,
          remainingLine: line,
          currentIndex: 0
        };

        semanticStackBuilder(docLine, json, stackType, tokensBuilder);
      });

      return tokensBuilder.build();
    }
  };

  languages.registerDocumentSemanticTokensProvider({
    language: 'callstack',
    pattern: fileUri
  }, provider, legend);
};

export const CLASS_DELIMITER = '.';
export const STRING_SPACE = ' ';
export const METHOD_BODY = '{ ... }';
export const parseJsonStack = (jsonStack: IStackTree[]) => jsonStack.map((line) => {
  const { method, fullClass } = line.typeValue;
  const endLineStringStack = [];
  if (line.typeDiscriminator === 1) {
    return line.typeValue.value;
  }

  if (fullClass) {
    const { genericParameterTypes } = fullClass;
    endLineStringStack.push(fullClass.shortClassNameOrDefault);
    if (genericParameterTypes && genericParameterTypes.length) {
      endLineStringStack.push(`<${genericParameterTypes.join(', ')}>${CLASS_DELIMITER}`);
    } else {
      endLineStringStack.push(CLASS_DELIMITER);
    }
  }
  if (method) {
    const { parameters, genericParameterTypes } = method;
    endLineStringStack.push(method.name);

    // Push type params if they exist
    if (genericParameterTypes && genericParameterTypes.length) {
      endLineStringStack.push(`<${genericParameterTypes.join(', ')}>`);
    }
    // Push method params
    const paramString = parameters.length ? parameters.map((param) => [
      param.parameterType && param.parameterType,
      param.parameterType && param.parameterName && STRING_SPACE,
      param.parameterName && param.parameterName
    ].join('')).join(', ') : '';
    endLineStringStack.push(`(${paramString}) ${METHOD_BODY}`);
  }
  return endLineStringStack.join('');
}).join('\n');