uniform extern float2 Size;
uniform extern float2 Root;
uniform extern float Radius;
uniform extern float SpeedPx = 300;
uniform extern float FadeHead = 50;
uniform extern float FadeTail = 200;
uniform extern float PulseW =50;
uniform extern float Time;



// this is the texture we are trying to render
uniform extern texture ScreenTexture;
sampler ScreenS = sampler_state
{
  // get the texture we are trying to render from the gpu.
  Texture = <ScreenTexture>;
};
 

float pExp(float x)
{
	return x / exp(x) / 0.36787944117144; //div by 1/e to normalize to max 1
}
float pExpNorm(float x)
{
	return pExp(x*3/PulseW);
}



float guass(float x)
{
	return 1/exp(x*x);
}

float quad(float x)
{
	return x*(1-x)/0.25;
}
float quadNorm(float x)
{
	float val = quad(x / PulseW);

	float decayCutoff = 0.75;
	if(x > decayCutoff*PulseW)
	{
		float cutMax = quad(decayCutoff);
		float p = 1-(x - decayCutoff*PulseW)/FadeTail;
		val = cutMax*p;
	}

	return val;
}
float decayQuadPulse(float r, float t)
{
	return quadNorm(SpeedPx*t - r);
}


float decayPulse(float r, float t)
{
	return pExpNorm(SpeedPx*t - r);
}



float4 PixelShaderFunction(float2 uvpos: TEXCOORD0) : COLOR0
{
	float2 pos = uvpos;
	pos.x *= Size.x;
	pos.y *= Size.y;

	float4 color =  float4(1,1,1,1);

	float2 relPos = pos - Root;
	float r = length(relPos);
	float alpha = decayQuadPulse(r, Time); //decayPulse(r, Time); //pExpNorm(r);

	if(r > Radius - FadeHead) //fade the edges, no hard edge at radius
	{
		//alpha *= 1 - ((r- (Radius - FadeHead))/FadeHead);
	}

	//alpha *= 1 - (r/Radius);
	alpha *= 1.5*guass(2*r/Radius);

	if(r > Radius)
	{
		alpha = 0;
	}

	return color * alpha;
}

float4 PixelShaderFunction2(float2 uvpos: TEXCOORD0) : COLOR0
{
	float2 pos = uvpos;
	pos.x *= Size.x;
	pos.y *= Size.y;

	float4 color =  float4(1,1,1,1);

	float2 relPos = pos - Root;
	float r = length(relPos);
	float alpha = decayPulse(r, Time); //pExpNorm(r);

	//if(r > Radius - FadeHead) //fade the edges, no hard edge at radius
	//{
	//	alpha *= 1 - ((r- (Radius - FadeHead))/FadeHead);
	//}

	//alpha *= 1 - (r/Radius);
	//alpha *= 1.5*guass(2*r/Radius);

	if(r > Radius)
	{
		alpha = 0;
	}

	return color * alpha;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
