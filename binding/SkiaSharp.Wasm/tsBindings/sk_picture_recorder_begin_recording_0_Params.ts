/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_picture_recorder_begin_recording_0_Params
	{
		/* Pack=4 */
		r : number;
		rect : SkiaSharp.SKRect;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_picture_recorder_begin_recording_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_picture_recorder_begin_recording_0_Params();
			
			{
				ret.r = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.rect = SkiaSharp.SKRect.unmarshal(pData + 4);
			}
			return ret;
		}
	}
}
