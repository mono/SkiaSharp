/* TSBindingsGenerator Generated code -- this code is regenerated on each build */
namespace SkiaSharp
{
	export class sk_vertices_make_copy_0_Params
	{
		/* Pack=4 */
		vmode : number;
		vertexCount : number;
		positions_Length : number;
		positions : Array<SkiaSharp.SKPoint>;
		texs_Length : number;
		texs : Array<SkiaSharp.SKPoint>;
		colors_Length : number;
		colors : Array<SkiaSharp.SKColor>;
		indexCount : number;
		indices_Length : number;
		indices : Array<number>;
		public static unmarshal(pData:number, memoryContext: any = null) : sk_vertices_make_copy_0_Params
		{
			memoryContext = memoryContext ? memoryContext : Module;
			let ret = new sk_vertices_make_copy_0_Params();
			
			{
				ret.vmode = Number(memoryContext.getValue(pData + 0, "i32"));
			}
			
			{
				ret.vertexCount = Number(memoryContext.getValue(pData + 4, "i32"));
			}
			
			{
				ret.positions_Length = Number(memoryContext.getValue(pData + 8, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 12, "*");
				if(pArray !== 0)
				{
					ret.positions = new Array<SkiaSharp.SKPoint>();
					for(var i=0; i<ret.positions_Length; i++)
					{
						ret.positions.push(SkiaSharp.SKPoint.unmarshal(pArray + i * 4));
					}
				}
				else
				
				{
					ret.positions = null;
				}
			}
			
			{
				ret.texs_Length = Number(memoryContext.getValue(pData + 16, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 20, "*");
				if(pArray !== 0)
				{
					ret.texs = new Array<SkiaSharp.SKPoint>();
					for(var i=0; i<ret.texs_Length; i++)
					{
						ret.texs.push(SkiaSharp.SKPoint.unmarshal(pArray + i * 4));
					}
				}
				else
				
				{
					ret.texs = null;
				}
			}
			
			{
				ret.colors_Length = Number(memoryContext.getValue(pData + 24, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 28, "*");
				if(pArray !== 0)
				{
					ret.colors = new Array<SkiaSharp.SKColor>();
					for(var i=0; i<ret.colors_Length; i++)
					{
						ret.colors.push(SkiaSharp.SKColor.unmarshal(pArray + i * 4));
					}
				}
				else
				
				{
					ret.colors = null;
				}
			}
			
			{
				ret.indexCount = Number(memoryContext.getValue(pData + 32, "i32"));
			}
			
			{
				ret.indices_Length = Number(memoryContext.getValue(pData + 36, "i32"));
			}
			
			{
				var pArray = memoryContext.getValue(pData + 40, "*");
				if(pArray !== 0)
				{
					ret.indices = new Array<number>();
					for(var i=0; i<ret.indices_Length; i++)
					{
						var value = memoryContext.getValue(pArray + i * 4, "i16");
						ret.indices.push(Number(value));
					}
				}
				else
				
				{
					ret.indices = null;
				}
			}
			return ret;
		}
	}
}
