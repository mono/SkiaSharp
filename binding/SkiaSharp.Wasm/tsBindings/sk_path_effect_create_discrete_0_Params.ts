/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_path_effect_create_discrete_0_Params
	{
		/* Pack=4 */
		segLength : number;
		deviation : number;
		seedAssist : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_path_effect_create_discrete_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_path_effect_create_discrete_0_Params();
			
			{
				ret.segLength = Number(memoryContext.getValue(pData + 0, "float"));
			}
			
			{
				ret.deviation = Number(memoryContext.getValue(pData + 4, "float"));
			}
			
			{
				ret.seedAssist = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			return ret;
		}
	}
}
