using System;

namespace KinectMvvm
{
    public interface IKinectService
    {
        event EventHandler<SkeletonEventArgs> SkeletonUpdated;
    }
}
