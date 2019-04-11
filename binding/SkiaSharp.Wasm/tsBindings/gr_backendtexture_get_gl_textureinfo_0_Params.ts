/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class gr_backendtexture_get_gl_textureinfo_0_Params
	{
		/* Pack=4 */
		texture : number;
		public static unmarshal(pData:number, memoryContext: any = null) : gr_backendtexture_get_gl_textureinfo_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new gr_backendtexture_get_gl_textureinfo_0_Params();
			
			{
				ret.texture = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
