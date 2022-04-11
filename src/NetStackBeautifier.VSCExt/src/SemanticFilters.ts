import { Position, Range, SemanticTokensBuilder } from "vscode";
import { IDocumentLine } from "./callStackLanguageService";
import { IStackTreeMethod } from "./StackDocumentProvider";

const buildRange = (line: number, startingPoint: number, endingPoint: number) => new Range(new Position(line, startingPoint), new Position(line, endingPoint));

const getRemainingLine = (docLine: IDocumentLine, startingPoint: number, endingPoint: number) => docLine.remainingLine.slice(0, startingPoint) + docLine.remainingLine.slice(endingPoint, docLine.remainingLine.length)

const updateDocumentLine = (docLine: IDocumentLine, startingPoint: number, endingPoint: number, matchLength: number) => {
    docLine.removals.push({
        removedAtIndex: startingPoint,
        removedLength: matchLength
    });
    docLine.remainingLine = getRemainingLine(docLine, startingPoint, endingPoint);
};

const getTrueStartingPoint = (docLine: IDocumentLine, startingPoint: number) =>
    startingPoint + docLine.removals
        .filter((removal) => removal.removedAtIndex <= startingPoint)
        .reduce((acc, curr) => (curr.removedLength + acc), 0);

const getFilterRange = (matchString: string, docLine: IDocumentLine, offSet: number = 0) => {
    const { remainingLine, lineNumber } = docLine;
    const startingPoint = remainingLine.indexOf(matchString);
    const endingPoint = startingPoint + matchString.length;

    const realStartingPoint = getTrueStartingPoint(docLine, startingPoint) + offSet;
    const realEndingPoint = matchString.length + realStartingPoint;
    updateDocumentLine(docLine, startingPoint, endingPoint, matchString.length);

    return buildRange(lineNumber, realStartingPoint, realEndingPoint);
};

const highlightLine = (builder: SemanticTokensBuilder, matchString: string, docLine: IDocumentLine, tokenType: string, tokenModifiers?: string[], offSet: number = 0) => {
    if (docLine.remainingLine.indexOf(matchString) !== -1) {
        builder.push(
            getFilterRange(matchString, docLine),
            tokenType,
            tokenModifiers
        );
    }
};

export function assemblySignature(signature: string, docLine: IDocumentLine, builder: SemanticTokensBuilder) {
    if (signature) {
        highlightLine(builder, signature, docLine, 'class', ['declaration']);
    }
}

export function classNameFilter(className: string, docLine: IDocumentLine, builder: SemanticTokensBuilder) {
    if (className) {
        highlightLine(builder, className, docLine, 'class', ['declaration']);
    }
}

export function classNameSections(nameSections: string[], docLine: IDocumentLine, builder: SemanticTokensBuilder) {
    if (nameSections && nameSections.length) {
        nameSections.map((nameSection) => {
            highlightLine(builder, nameSection, docLine, 'class', ['declaration']);
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

        highlightLine(builder, matchString, docLine, 'interface', ['declaration']);
    });
};

export function methodName(method: IStackTreeMethod, docLine: IDocumentLine, builder: SemanticTokensBuilder) {
    if (method && method.name) {
        highlightLine(builder, method.name, docLine, 'function', ['declaration']);
    }
}
export const STRING_SPACE = ' ';
export function methodParameterType(method: IStackTreeMethod, docLine: IDocumentLine, builder: SemanticTokensBuilder, rawParam: boolean = false) {
    if (method && method.parameters && method.parameters.length) {
        method.parameters.forEach((param) => {
            const parameterType = rawParam ? param.rawParameterType : param.parameterType;
            const parameterName = rawParam ? param.fullParameterName : param.parameterName;
            if (parameterType && parameterType.length) {
                highlightLine(builder, parameterType, docLine, 'interface', ['defaultLibrary']);
            }
            if (parameterName && parameterName.length) {
                highlightLine(builder, parameterType, docLine, 'parameter', ['declaration']);
            }
        });
    }
}

export function rawGenericParameterType(rawGenericParameterTypes: string[], docLine: IDocumentLine, builder: SemanticTokensBuilder) {
    if (rawGenericParameterTypes && rawGenericParameterTypes.length) {
        rawGenericParameterTypes.forEach((typeParam) => {
            highlightLine(builder, typeParam, docLine, 'interface', ['declaration']);
        });
    }
}
