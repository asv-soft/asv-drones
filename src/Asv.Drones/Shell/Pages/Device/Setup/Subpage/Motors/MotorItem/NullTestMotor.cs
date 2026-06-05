using Asv.Mavlink;
using Asv.Mavlink.Common;
using R3;

namespace Asv.Drones;

public class NullTestMotor : ITestMotor
{
    public static ITestMotor Instance { get; } = new NullTestMotor(1, 4, true, 100);

    public NullTestMotor(int id, int servoChannel, bool isTestRun, ushort pwm)
    {
        Id = id;
        ServoChannel = servoChannel;
        IsTestRun = new ReactiveProperty<bool>(isTestRun).ToReadOnlyReactiveProperty();
        Pwm = new ReactiveProperty<ushort>(pwm).ToReadOnlyReactiveProperty();
    }

    public int Id { get; }
    public int ServoChannel { get; }
    public ReadOnlyReactiveProperty<bool> IsTestRun { get; }
    public ReadOnlyReactiveProperty<ushort> Pwm { get; }

    public Task<MavResult> StartTest(int throttle, int timeout, CancellationToken cancel = default)
    {
        return Task.FromResult(MavResult.MavResultAccepted);
    }

    public Task<MavResult> StopTest(CancellationToken cancel = default)
    {
        return Task.FromResult(MavResult.MavResultAccepted);
    }
}
