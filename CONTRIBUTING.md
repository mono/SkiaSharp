Guidelines
==========

When contributing to the Mono project, please follow the [Mono Coding
Guidelines][1].  We have been using a coding style for many years,
please make your patches conform to these guidelines.

[1]: http://www.mono-project.com/community/contributing/coding-guidelines/

Etiquette
=========

In general, we do not accept patches that merely shuffle code around,
split classes in multiple files, reindent the code or are the result
of running a refactoring tool on the source code.  This is done for
three reasons: (a) we have our own coding guidelines; (b) Some modules
are imported from upstream sources and we want to respect their coding
guidelines and (c) it destroys valuable history that is often used to
investigate bugs, regressions and problems.
