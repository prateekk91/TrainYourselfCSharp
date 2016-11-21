using System;
using System.Linq;
using Microsoft.Kinect;
using System.Collections.Generic;

namespace KinectMvvm
{
    public class ConcreteKinectService : IKinectService
    {
        public event EventHandler<SkeletonEventArgs> SkeletonUpdated;

        KinectSensor sensor;
        private BodyFrameReader bodyFrameReader = null;
        private Body[] bodies = null;
        private int bodyIndex;
        private bool bodyTracked = false;
        private static int frameCount = 0;

        public ConcreteKinectService()
        {
            this.sensor = KinectSensor.GetDefault();
            this.bodyFrameReader = this.sensor.BodyFrameSource.OpenReader();
            this.sensor.Open();
            this.bodyFrameReader.FrameArrived += this.FrameArrived;
        }

        private void FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }
            
            if (dataReceived)
            {
                Body body = null;

                // Check if current body tracked is the same as the previous body tracked
                if (this.bodyTracked)
                {
                    if (this.bodies[this.bodyIndex].IsTracked)
                    {
                        body = this.bodies[this.bodyIndex];
                    }
                    else
                    {
                        this.bodyTracked = false;
                    }
                }

                // Identify the current body being tracked
                if (!this.bodyTracked)
                {
                    for (int i = 0; i < this.bodies.Length; ++i)
                    {
                        if (this.bodies[i].IsTracked)
                        {
                            this.bodyIndex = i;
                            this.bodyTracked = true;
                            break;
                        }
                    }
                }
                
                // Do something with body
                if (body != null && this.bodyTracked && body.IsTracked)
                {
                    if (this.SkeletonUpdated != null)
                    {
                        if (frameCount == 5)
                        {
                            this.SkeletonUpdated(this, new SkeletonEventArgs() { TrackedBody = body });
                            frameCount = 0;
                        }
                        frameCount++;
                    }
                }   
            }
        }
    }
}
