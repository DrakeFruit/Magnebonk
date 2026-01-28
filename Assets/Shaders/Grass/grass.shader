HEADER
{
    Description = "Grass Compute";
}

FEATURES
{

}

MODES
{
    Default(); 
}

CS
{
    #include "system.fxc"

    struct BladeData {
        float4 position;
        float4 wind;
    };

    // 1. Ensure this is RWStructuredBuffer
    // 2. Make sure the Attribute name matches your C# shader.Attributes.Set call
    StructuredBuffer<BladeData> AllBlades < Attribute( "AllBlades" ); >;
    RWStructuredBuffer<BladeData> VisibleBlades < Attribute( "VisibleBlades" ); >;
    
    // Using a uint buffer for the indirect args
    RWStructuredBuffer<uint> DrawArgs < Attribute( "DrawArgs" ); >;

    [numthreads(64, 1, 1)]
    void MainCs( uint3 id : SV_DispatchThreadID )
    {
        uint totalBlades, stride;
        AllBlades.GetDimensions(totalBlades, stride);
        if (id.x >= totalBlades) return;

        BladeData blade = AllBlades[id.x];

        // Replace this with your actual culling math (frustum/distance)
        bool isVisible = true; 

        if (isVisible)
        {
            uint writeIndex;
            // DrawArgs[1] is the instanceCount in your IndirectCommand struct
            InterlockedAdd(DrawArgs[1], 1, writeIndex);

            // This should now work without the 'const' error
            VisibleBlades[writeIndex] = blade;
        }
    }
}