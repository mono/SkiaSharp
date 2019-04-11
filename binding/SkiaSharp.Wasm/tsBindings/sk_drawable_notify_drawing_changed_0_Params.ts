/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_drawable_notify_drawing_changed_0_Params
	{
		/* Pack=4 */
		d : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_drawable_notify_drawing_changed_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_drawable_notify_drawing_changed_0_Params();
			
			{
				ret.d = Number(memoryContext.getValue(pData + 0, "*"));
			}
			return ret;
		}
	}
}
