// this is the texture we are trying to render
uniform extern texture ScreenTexture;
sampler ScreenS = sampler_state
{
  // get the texture we are trying to render from the gpu.
  Texture = <ScreenTexture>;
};
 
// this is the alpha map texture, we set this from the C# code.
uniform extern texture MaskTexture;
sampler MaskS = sampler_state
{
  Texture = <MaskTexture>;
};

float4 PixelShaderFunction(float2 texCoord: TEXCOORD0) : COLOR0
{
    //return float4(1, 0, 0, 1);

	float4 color = tex2D(ScreenS, texCoord);

	float alpha = tex2D(MaskS, texCoord).r;

	return color * alpha;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
