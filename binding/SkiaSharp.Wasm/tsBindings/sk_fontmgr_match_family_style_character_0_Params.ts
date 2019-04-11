/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_fontmgr_match_family_style_character_0_Params
	{
		/* Pack=4 */
		fontmgr : number;
		familyName : string;
		style : number;
		bcp47_Length : number;
		bcp47 : Array<string>;
		bcp47Count : number;
		character : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_fontmgr_match_family_style_character_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_fontmgr_match_family_style_character_0_Params();
			
			{
				ret.fontmgr = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				var ptr = memoryContext.getValue(pData + 4, "*");
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
				ret.style = Number(memoryContext.getValue(pData + 8, "*"));
			}
			
			{
				ret.bcp47_Length = Number(memoryContext.getValue(pData + 12, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 16, "*"); /*string 4 True*/
				if(pArray !== 0)
				{
					ret.bcp47 = new Array<string>();
					for(var i=0; i<ret.bcp47_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 4, "*");
						if(value !== 0)
						{
							ret.bcp47.push(String(MonoRuntime.conv_string(value)));
						}
						else
						
						{
							ret.bcp47.push(null);
						}
					}
				}
				else
				
				{
					ret.bcp47 = null;
				}
			}
			
			{
				ret.bcp47Count = Number(memoryContext.getValue(pData + 20, "i32"));
			}
			
			{
				ret.character = Number(memoryContext.getValue(pData + 24, "i32"));
			}
			return ret;
		}
	}
}
