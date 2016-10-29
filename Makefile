all:
	echo You can use "make update-docs" to update the docs, "or make assemble-docs" to assemble for deployment

update-docs:
	echo Not supported anymore, just "make assemble-docs"

assemble-docs:
	mdoc assemble --out=docs/SkiaSharp docs/en
