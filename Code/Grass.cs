using Sandbox;
using Sandbox.Rendering;
using System.Runtime.InteropServices;

public sealed class GrassRenderer : Component
{
    private ComputeShader GrassShader = new ComputeShader( "shaders/grass/grass.shader" );
    
    private GpuBuffer<BladeData> _allBladesBuffer;
    private GpuBuffer<BladeData> _visibleBladesBuffer;
    private GpuBuffer<IndirectCommand> _indirectBuffer;

    private RenderAttributes _renderAttributes = new RenderAttributes();

    protected override void OnStart()
    {
        _allBladesBuffer = new GpuBuffer<BladeData>( 100000, GpuBuffer.UsageFlags.Structured );
        _visibleBladesBuffer = new GpuBuffer<BladeData>( 100000, GpuBuffer.UsageFlags.Structured );
        _indirectBuffer = new GpuBuffer<IndirectCommand>( 1, GpuBuffer.UsageFlags.IndirectDrawArguments | GpuBuffer.UsageFlags.Structured );
        
        ResetIndirectBuffer();
    }

    private void ResetIndirectBuffer()
    {
        _indirectBuffer.SetData( new[] { new IndirectCommand { indexCount = 6 } } );
    }

    protected override void OnPreRender()
    {
        if ( GrassShader is null || _allBladesBuffer is null ) return;

        ResetIndirectBuffer();

        _renderAttributes.Set( "AllBlades", _allBladesBuffer );
        _renderAttributes.Set( "VisibleBlades", _visibleBladesBuffer );
        _renderAttributes.Set( "DrawArgs", _indirectBuffer );

        GrassShader.DispatchWithAttributes( _renderAttributes, 100000 / 64, 1, 1 );

        if ( Model.Plane is not null )
        {
            Graphics.DrawModelInstancedIndirect( Model.Plane, _indirectBuffer, default, _renderAttributes );
        }
    }

    public struct BladeData {
        public Vector4 position;
        public Vector4 wind;
    }

    public struct IndirectCommand {
        public uint indexCount;
        public uint instanceCount;
        public uint firstIndex;
        public int vertexOffset;
        public uint firstInstance;
    }
}