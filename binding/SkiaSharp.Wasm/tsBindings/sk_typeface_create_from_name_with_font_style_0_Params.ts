/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_typeface_create_from_name_with_font_style_0_Params
	{
		/* Pack=4 */
		familyName : string;
		style : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_typeface_create_from_name_with_font_style_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_typeface_create_from_name_with_font_style_0_Params();
			
			{
				var ptr = memoryContext.getValue(pData + 0, "*");
				if(ptr !== 0)
				{
					ret.familyName = String(memoryContext.UTF8ToString(ptr));
				}
				else
				
				{
					ret.familyName = null;
				}
			}
			
			{
				ret.style = Number(memoryContext.getValue(pData + 4, "*"));
			}
			return ret;
		}
	}
}
