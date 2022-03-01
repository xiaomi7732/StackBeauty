# Stack Beauty

.NET Call Stack is a chuck of text that is not easy to read.
And we shall try to make a change there.

## Vision

This is going to be a service, taking in a chuck of text, understand the type of the call stack, beautify it and then return.

## Live site

[Stack Beauty](http://stackbeauty.codewithsaar.net/)

## Challenge

There are various formats for call stack the user could get. And that shouldn't matter to the user. As long as it is a .NET stack, this service shall be able to recognize it, beautify it and return the result to the user.

## Architecture

```mermaid
graph LR
    A[Input] -->|Parsers| B(IFrameLine collection)
    B -->|Beautifiers| C{Beautified IFrameLine collection}
    C -->|Html Render| D[Html]
    C -->|Text Render| E[Text]
    C -->|... Render| F[...]
```

Generally, once input, callstack will be parsed by parsers; then beautifiers steps in to beautify various parts and output it as json; renders will pick up the json and output it to various results - html; svg; ...

```mermaid
graph TD
    A[Stack Blob] -->|Match| B{Find out beautifier}
    B --> |exception| C[Exception trace beautify]
    B --> |ETW trace| D[ETW trace beautify]
    B --> |...| E[...]
    B --> |Stream| F[Stream trace beautify]
    F
    C --> Z[output AsyncEnumerable of FrameItem]
    D --> Z
    E --> Z
    F --> Z
```

