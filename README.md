# Dependencies Analyzer

A tool created for .NET that analyze dependencies of components affected into a feature branch.

This tool should indicate wich files have changing relative to your remote master.

The main idea is that every time you create or modify methods in development. At the same time these functions are invoked by other functions by creating a tree of invocations that ends at the initial point of the execution of an action.

Obtaining the dependency tree and finding the initial entry points can identify them with an attribute that describes the affected business functionality.

This is a great help for quality areas to perform very specific regression tests.

Also this tool will graph the dependencies of the method calls in a visual form for a better understanding of the modifications.

## General Considerations

The tool is a desktop program for OS Windows and uses C# language for development and WPF Framework.

The tool requires having GIT installed and an account to some repository.

The source code of the tool is free so that it can be used and modified by the interested community.

The tool does not occupy licensed software so it should be kept that way for the future.

### Enjoy it!
