/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_shader_new_compose_with_mode_0_Params
	{
		/* Pack=4 */
		shaderA : number;
		shaderB : number;
		mode : number;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_shader_new_compose_with_mode_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_shader_new_compose_with_mode_0_Params();
			
			{
				ret.shaderA = Number(memoryContext.getValue(pData + 0, "*"));
			}
			
			{
				ret.shaderB = Number(memoryContext.getValue(pData + 4, "*"));
			}
			
			{
				ret.mode = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			return ret;
		}
	}
}
