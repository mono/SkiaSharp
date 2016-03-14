all:
	echo You can use "make update-docs" to update the docs, "or make assemble-docs" to assemble for deployment

update-docs:
	(cd binding; xbuild SkiaSharp.Generic.sln)
	mdoc update --out=docs/en binding/SkiaSharp.Generic/bin/Debug/SkiaSharp.dll
	(cd docs; mdoc assemble --out=SkiaSharp en)

assemble-docs:
	mdoc assemble --out=docs/SkiaSharp docs/en
