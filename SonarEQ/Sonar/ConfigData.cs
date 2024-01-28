using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonarEQ.Sonar
{
    public class BassBoostState
    {
        public bool enabled { get; set; }
        public double value { get; set; }
    }

    public class Center
    {
        public double position { get; set; }
        public double gain { get; set; }
    }

    public class Filter
    {
        public bool enabled { get; set; }
        public double qFactor { get; set; }
        public double frequency { get; set; }
        public double gain { get; set; }
        public string type { get; set; } = string.Empty;
    }

    public class FrontLeft
    {
        public double position { get; set; }
        public double gain { get; set; }
    }

    public class FrontRight
    {
        public double position { get; set; }
        public double gain { get; set; }
    }

    public class ParametricEQ
    {
        public bool enabled { get; set; }
        public Filter filter1 { get; set; }
        public Filter filter2 { get; set; }
        public Filter filter3 { get; set; }
        public Filter filter4 { get; set; }
        public Filter filter5 { get; set; }
        public Filter filter6 { get; set; }
        public Filter filter7 { get; set; }
        public Filter filter8 { get; set; }
        public Filter filter9 { get; set; }
        public Filter filter10 { get; set; }

        public ParametricEQ()
        {
            filter1 = new Filter();
            filter2 = new Filter();
            filter3 = new Filter();
            filter4 = new Filter();
            filter5 = new Filter();
            filter6 = new Filter();
            filter7 = new Filter();
            filter8 = new Filter();
            filter9 = new Filter();
            filter10 = new Filter();
        }
    }

    public class RearLeft
    {
        public double position { get; set; }
        public double gain { get; set; }
    }

    public class RearRight
    {
        public double position { get; set; }
        public double gain { get; set; }
    }

    public class SonarPreset
    {
        public BassBoostState bassBoostState { get; set; }
        public TrebleBoostState trebleBoostState { get; set; }
        public VoiceClarityState voiceClarityState { get; set; }
        public SmartVolume smartVolume { get; set; }
        public double generalGain { get; set; }
        public ParametricEQ parametricEQ { get; set; }
        public bool virtualSurroundState { get; set; }
        public VirtualSurroundChannels virtualSurroundChannels { get; set; }
        public double reverbGainDB { get; set; }
        public string formFactor { get; set; } = string.Empty;
        public bool globalEnableState { get; set; }

        public SonarPreset()
        {
            bassBoostState = new BassBoostState();
            trebleBoostState = new TrebleBoostState();
            voiceClarityState = new VoiceClarityState();
            smartVolume = new SmartVolume();
            parametricEQ = new ParametricEQ();
            virtualSurroundChannels = new VirtualSurroundChannels();
        }
    }

    public class SideLeft
    {
        public double position { get; set; }
        public double gain { get; set; }
    }

    public class SideRight
    {
        public double position { get; set; }
        public double gain { get; set; }
    }

    public class SmartVolume
    {
        public bool enabled { get; set; }
        public double volumeLevel { get; set; }
        public string loudness { get; set; } = string.Empty;
    }

    public class SubWoofer
    {
        public double position { get; set; }
        public double gain { get; set; }
    }

    public class TrebleBoostState
    {
        public bool enabled { get; set; }
        public double value { get; set; }
    }

    public class VirtualSurroundChannels
    {
        public FrontLeft frontLeft { get; set; }
        public FrontRight frontRight { get; set; }
        public Center center { get; set; }
        public SubWoofer subWoofer { get; set; }
        public RearLeft rearLeft { get; set; }
        public RearRight rearRight { get; set; }
        public SideLeft sideLeft { get; set; }
        public SideRight sideRight { get; set; }

        public VirtualSurroundChannels()
        {
            frontLeft = new FrontLeft();
            frontRight = new FrontRight();
            center = new Center();
            subWoofer = new SubWoofer();
            rearLeft = new RearLeft();
            rearRight = new RearRight();
            sideLeft = new SideLeft();
            sideRight = new SideRight();
        }
    }

    public class VoiceClarityState
    {
        public bool enabled { get; set; }
        public double value { get; set; }
    }
}
