/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_codec_get_frame_info_0_Params
	{
		/* Pack=4 */
		codec : number;
		frameInfo_Length : number;
		frameInfo : Array<SkiaSharp.SKCodecFrameInfo>;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_codec_get_frame_info_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_codec_get_frame_info_0_Params();
			
			{
				ret.codec = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.frameInfo_Length = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 8, "*");
				if(pArray !== 0)
				{
					ret.frameInfo = new Array<SkiaSharp.SKCodecFrameInfo>();
					for(var i=0; i<ret.frameInfo_Length; i++)
					{
						ret.frameInfo.push(SkiaSharp.SKCodecFrameInfo.unmarshal(pArray + i * 4));
					}
				}
				else
				
				{
					ret.frameInfo = null;
				}
			}
			return ret;
		}
	}
}
