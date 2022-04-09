import { Position, Range, SemanticTokensBuilder } from "vscode";
import { IDocumentLine } from "./callStackLanguageService";
import { IStackTreeMethod } from "./StackDocumentProvider";

const buildRange = (line: number, startingPoint: number, endingPoint: number) => new Range(new Position(line, startingPoint), new Position(line, endingPoint));

const updateDocumentLine = (docLine: IDocumentLine, endingPoint: number) => {
    docLine.currentIndex = endingPoint;
    docLine.remainingLine = docLine.fullLine.slice(endingPoint);
};

const getFilterRange = (matchString: string, docLine: IDocumentLine, offSet: number = 0) => {
    const { currentIndex, remainingLine, lineNumber } = docLine;
    const startingPoint = currentIndex + remainingLine.indexOf(matchString) + offSet;
    const endingPoint = startingPoint + matchString.length;
    updateDocumentLine(docLine, endingPoint);

    return buildRange(lineNumber, startingPoint, endingPoint);
};

export function assemblySignature(signature: string, docLine: IDocumentLine, builder: SemanticTokensBuilder) {
    if (signature) {
        const startingPoint = docLine.fullLine.indexOf(signature);
        const endingPoint = startingPoint + signature.length;
        builder.push(
            buildRange(docLine.lineNumber, startingPoint, endingPoint),
            'class',
            ['declaration']
        );
    }
}

export function classNameFilter(className: string, docLine: IDocumentLine, builder: SemanticTokensBuilder) {
    if (className) {
        builder.push(
            getFilterRange(className, docLine),
            'class',
            ['declaration']
        );
    }
}

export function classNameSections(nameSections: string[], docLine: IDocumentLine, builder: SemanticTokensBuilder) {
    if (nameSections && nameSections.length) {
        nameSections.map((nameSection) => {
            builder.push(
                getFilterRange(nameSection, docLine),
                'class',
                ['declaration']
            );
        });
    }
}

export function genericParamaterTypeBuilder(genericParameterTypes: string[], docLine: IDocumentLine, builder: SemanticTokensBuilder) {
    genericParameterTypes.forEach((typeParam, typeIndex) => {
        let matchString;
        let offSet;
        if (typeIndex === 0) {
            matchString = '<' + typeParam;
            offSet = 1;
        } else if (typeIndex + 1 === genericParameterTypes.length) {
            matchString = typeParam + '>';
        } else {
            matchString = typeParam + ', ' + '>';
            offSet = 2;
        }

        builder.push(
            getFilterRange(matchString, docLine, offSet),
            'interface',
            ['declaration']
        );
    });
};

export function methodName(method: IStackTreeMethod, docLine: IDocumentLine, builder: SemanticTokensBuilder) {
    if (method && method.name) {
        builder.push(
            getFilterRange(method.name, docLine),
            'function',
            ['declaration']
        );
    }
}
export const STRING_SPACE = ' ';
export function methodParameterType(method: IStackTreeMethod, docLine: IDocumentLine, builder: SemanticTokensBuilder, rawParam: boolean = false) {
    if (method && method.parameters && method.parameters.length) {
        method.parameters.forEach((param) => {
            const parameterType = rawParam ? param.rawParameterType : param.parameterType;
            const parameterName = rawParam ? param.fullParameterName : param.parameterName;
            if (parameterType && parameterType.length) {
                builder.push(
                    getFilterRange(parameterType, docLine),
                    'interface',
                    ['defaultLibrary']
                );
            }
            if (parameterName && parameterName.length) {
                builder.push(
                    getFilterRange(parameterName, docLine),
                    'parameter',
                    ['declaration']
                );
            }
        });
    }
}

export function rawGenericParameterType(rawGenericParameterTypes: string[], docLine: IDocumentLine, builder: SemanticTokensBuilder) {
    if (rawGenericParameterTypes && rawGenericParameterTypes.length) {
        rawGenericParameterTypes.forEach((typeParam) => {    
            builder.push(
                getFilterRange(typeParam, docLine),
                'interface',
                ['declaration']
            );
        });
    }
}
