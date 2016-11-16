using System;
using Microsoft.Kinect;

namespace KinectMvvm
{
    public class SkeletonEventArgs : EventArgs
    {
        public Body TrackedBody { get; set; }
    }
}
