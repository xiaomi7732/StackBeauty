import {
    CancellationToken,
    DocumentSemanticTokensProvider,
    EventEmitter,
    languages,
    Position,
    Range,
    SemanticTokensBuilder,
    SemanticTokensLegend,
    TextDocument,
    TextDocumentContentProvider,
    Uri
} from 'vscode';
import { CLASS_DELIMITER, IStackTree, STRING_SPACE } from './extension';

export class StackDocumentProvider implements TextDocumentContentProvider {
    public onDidChangeEmitter = new EventEmitter<Uri>();
    public onDidChange = this.onDidChangeEmitter.event;

    public provideTextDocumentContent(uri: Uri, _token: CancellationToken): string {
        return uri.path.trim();
    }
}

export const createSyntaxHighlighting = (json: IStackTree[]) => {
    const tokenTypes = ['class', 'parameter', 'function', 'interface', 'label', 'typeParameter', 'comment'];
    const tokenModifiers = ['declaration', 'documentation', 'definition', 'defaultLibrary'];
    const legend = new SemanticTokensLegend(tokenTypes, tokenModifiers);
    const buildRange = (line: number, startingPoint: number, endingPoint: number) => new Range(new Position(line, startingPoint), new Position(line, endingPoint));
    
    const genericParamaterTypeBuilder = (genericParameterTypes: string[], line: string, lineNumber: number, builder: SemanticTokensBuilder) => {
        genericParameterTypes.forEach((typeParam, typeIndex) => {
            let typeParamStartingPoint;
            let typeParamEndingPoint;
            if (typeIndex === 0) {
                typeParamStartingPoint = line.indexOf('<' + typeParam) + 1;
                typeParamEndingPoint = typeParamStartingPoint + typeParam.length;
                
            } else if (typeIndex + 1 === genericParameterTypes.length) {
                typeParamStartingPoint = line.indexOf(typeParam + '>');
                typeParamEndingPoint = typeParamStartingPoint + typeParam.length;
            } else {
                typeParamStartingPoint = line.indexOf(', ' + typeParam) + 2;
                typeParamEndingPoint = typeParamStartingPoint + typeParam.length;
            }

            builder.push(
                buildRange(lineNumber, typeParamStartingPoint, typeParamEndingPoint),
                'interface',
                ['declaration']
            );
        });
    };

    // Maybe move this into where we parse the json to begin with. Only problem is it feels like we'd be abandoning the spirit of the semantic tokenizer
    const provider: DocumentSemanticTokensProvider = {
        provideDocumentSemanticTokens(document: TextDocument, _token: CancellationToken) {
            const tokensBuilder = new SemanticTokensBuilder(legend);
            document.getText().split('\n').forEach((line, i) => {
                const jsonLine = json[i];
                const { fullClass, method } = jsonLine.typeValue;

                if (jsonLine.typeDiscriminator === 2) {
                    if (fullClass) {
                        if (fullClass.shortClassNameOrDefault) {
                            // Somewhat redundant since grammar regex generally matches correctly
                            const startingPoint = line.indexOf(fullClass.shortClassNameOrDefault);
                            const endingPoint = fullClass.shortClassNameOrDefault.length;
                            tokensBuilder.push(
                                buildRange(i, startingPoint, endingPoint),
                                'class',
                                ['declaration']
                            );
                        }
                        
                        if (fullClass.genericParameterTypes) {
                            genericParamaterTypeBuilder(fullClass.genericParameterTypes, line, i, tokensBuilder);
                        }
                    }
                    if (method) {
                        let methodStartingPoint;
                        let methodEndingPoint;
                        if (fullClass && fullClass.shortClassNameOrDefault) {
                            const genericParameterTypeString = fullClass.genericParameterTypes.length ? `<${fullClass.genericParameterTypes.join(', ')}>` : '';
                            const fullMethodString = fullClass.shortClassNameOrDefault + genericParameterTypeString + CLASS_DELIMITER + method.name;
                            methodStartingPoint = fullMethodString.length - method.name.length;
                            methodEndingPoint = fullMethodString.length;
                        } else {
                            methodStartingPoint = line.indexOf(method.name);
                            methodEndingPoint = method.name.length;
                        }
                        tokensBuilder.push(
                            buildRange(i, methodStartingPoint, methodEndingPoint),
                            'function',
                            ['declaration']
                        );

                        if (method.genericParameterTypes.length) {
                            genericParamaterTypeBuilder(method.genericParameterTypes, line, i, tokensBuilder);
                        }

                        if (method.parameters.length) {
                            method.parameters.forEach((param) => {
                                if (param.parameterName && param.parameterType) {
                                    const fullParamString = param.parameterType + STRING_SPACE + param.parameterName;
                                    const paramTypeStartingPoint = line.indexOf(fullParamString);
                                    const paramNameStartingPoint = paramTypeStartingPoint + param.parameterType.length + STRING_SPACE.length;
                                    const paramTypeEndingPoint = paramTypeStartingPoint + param.parameterType.length;
                                    const paramNameEndingPoint = paramNameStartingPoint + param.parameterName.length;
                                    tokensBuilder.push(
                                        buildRange(i, paramTypeStartingPoint, paramTypeEndingPoint),
                                        'interface',
                                        ['defaultLibrary']
                                    );
                                    tokensBuilder.push(
                                        buildRange(i, paramNameStartingPoint, paramNameEndingPoint),
                                        'parameter',
                                        ['declaration']
                                    );
                                } else if (param.parameterName) {
                                    const paramNameStartingPoint = line.indexOf(param.parameterName);
                                    const paramNameEndingPoint = paramNameStartingPoint + param.parameterName.length;
                                    tokensBuilder.push(
                                        buildRange(i, paramNameStartingPoint, paramNameEndingPoint),
                                        'parameter',
                                        ['declaration']
                                    );
                                } else {
                                    const paramTypeStartingPoint = line.indexOf(param.parameterType);
                                    const paramTypeEndingPoint = paramTypeStartingPoint + param.parameterType.length;
                                    tokensBuilder.push(
                                        buildRange(i, paramTypeStartingPoint, paramTypeEndingPoint),
                                        'interface',
                                        ['defaultLibrary']
                                    );
                                }
                            });
                        }
                    }
                } else {
                    tokensBuilder.push(
                        buildRange(i, 0, line.length),
                        'comment',
                        ['definition']
                    );
                }
            });
        
            return tokensBuilder.build();
        }
    };
    languages.registerDocumentSemanticTokensProvider({
        language: 'callstack'
    }, provider, legend);
};
