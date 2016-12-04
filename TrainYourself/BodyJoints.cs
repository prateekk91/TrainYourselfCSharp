using LightBuzz.Vitruvius;
using Microsoft.Kinect;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinectMvvm
{
    [Serializable]
    [JsonConverter(typeof(SkeletonSerializer))]
    public class BodyJoints
    {
        Dictionary<JointType, Position> joints;
        public static Tuple<JointType, double> maxDeviatedJoint;

        [Serializable]
        public class Position
        {
            public float x;
            public float y;
            public float z;

            public Position(float x_, float y_, float z_)
            {
                x = x_;
                y = y_;
                z = z_;
            }
        }

        public Joint getJoint(JointType jointType)
        {
            Joint joint = new Joint();
            Position pos = joints[jointType];
            joint.Position.X = pos.x;
            joint.Position.Y = pos.y;
            joint.Position.Z = pos.z;
            joint.JointType = jointType;

            return joint;
        }

        public static double operator -(BodyJoints trackedBody, BodyJoints idealBody)
        {
            double totalDeviation = 0;
            maxDeviatedJoint = new Tuple<JointType, double>(JointType.Head, 0.0);
            var trackedBodyAngle = GetAngle(trackedBody, JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight);
            var idealBodyAngle = GetAngle(idealBody, JointType.ShoulderRight, JointType.ElbowRight, JointType.WristRight);
            var deviation = Math.Abs(idealBodyAngle - trackedBodyAngle);
            totalDeviation += deviation;
            updateMaxDeviation(JointType.ElbowRight, deviation);

            trackedBodyAngle = GetAngle(trackedBody, JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft);
            idealBodyAngle = GetAngle(idealBody, JointType.ShoulderLeft, JointType.ElbowLeft, JointType.WristLeft);
            deviation = Math.Abs(idealBodyAngle - trackedBodyAngle);
            totalDeviation += deviation;
            updateMaxDeviation(JointType.ElbowLeft, deviation);

            trackedBodyAngle = GetAngle(trackedBody, JointType.ShoulderLeft, JointType.ShoulderRight, JointType.ElbowRight);
            idealBodyAngle = GetAngle(idealBody, JointType.ShoulderLeft, JointType.ShoulderRight, JointType.ElbowRight);
            deviation = Math.Abs(idealBodyAngle - trackedBodyAngle);
            totalDeviation += deviation;
            updateMaxDeviation(JointType.ShoulderRight, deviation);

            trackedBodyAngle = GetAngle(trackedBody, JointType.ShoulderRight, JointType.ShoulderLeft, JointType.ElbowLeft);
            idealBodyAngle = GetAngle(idealBody, JointType.ShoulderRight, JointType.ShoulderLeft, JointType.ElbowLeft);
            deviation = Math.Abs(idealBodyAngle - trackedBodyAngle);
            totalDeviation += deviation;
            updateMaxDeviation(JointType.ShoulderLeft, deviation);

            trackedBodyAngle = GetAngle(trackedBody, JointType.HipRight, JointType.ShoulderRight, JointType.ElbowRight);
            idealBodyAngle = GetAngle(idealBody, JointType.HipRight, JointType.ShoulderRight, JointType.ElbowRight);
            deviation = Math.Abs(idealBodyAngle - trackedBodyAngle);
            totalDeviation += deviation;
            updateMaxDeviation(JointType.ShoulderRight, deviation);

            trackedBodyAngle = GetAngle(trackedBody, JointType.HipLeft, JointType.ShoulderLeft, JointType.ElbowLeft);
            idealBodyAngle = GetAngle(idealBody, JointType.HipLeft, JointType.ShoulderLeft, JointType.ElbowLeft);
            deviation = Math.Abs(idealBodyAngle - trackedBodyAngle);
            totalDeviation += deviation;
            updateMaxDeviation(JointType.ShoulderLeft, deviation);

            trackedBodyAngle = GetAngle(trackedBody, JointType.KneeRight, JointType.HipRight, JointType.Neck);
            idealBodyAngle = GetAngle(idealBody, JointType.KneeRight, JointType.HipRight, JointType.Neck);
            deviation = Math.Abs(idealBodyAngle - trackedBodyAngle);
            totalDeviation += deviation;
            updateMaxDeviation(JointType.HipRight, deviation);

            trackedBodyAngle = GetAngle(trackedBody, JointType.KneeLeft, JointType.HipLeft, JointType.Neck);
            idealBodyAngle = GetAngle(idealBody, JointType.KneeLeft, JointType.HipLeft, JointType.Neck);
            deviation = Math.Abs(idealBodyAngle - trackedBodyAngle);
            totalDeviation += deviation;
            updateMaxDeviation(JointType.HipLeft, deviation);

            return totalDeviation;
        }

        private static void updateMaxDeviation(JointType joint, double deviation)
        {
            if (maxDeviatedJoint.Item2 < deviation)
            {
                maxDeviatedJoint = Tuple.Create(joint, deviation);
            }
        }

        private Position getJointPositions(Joint joint)
        {
            return new Position(joint.Position.X, joint.Position.Y, joint.Position.Z);
        }

        public int getNumJoints()
        {
            return joints.Count;
        }

        public void Translate(float[] diff)
        {
            foreach (KeyValuePair<JointType, Position> entry in joints)
            {
                entry.Value.x += diff[0];
                entry.Value.y += diff[1];
                entry.Value.z += diff[2];
            }
        }

        public BodyJoints(Body body)
        {
            joints = new Dictionary<JointType, Position>();
            joints.Add(JointType.Head, getJointPositions(body.Joints[JointType.Head]));

            joints.Add(JointType.Neck, getJointPositions(body.Joints[JointType.Neck]));

            joints.Add(JointType.ShoulderRight, getJointPositions(body.Joints[JointType.ShoulderRight]));
            joints.Add(JointType.ShoulderLeft, getJointPositions(body.Joints[JointType.ShoulderLeft]));

            joints.Add(JointType.SpineMid, getJointPositions(body.Joints[JointType.SpineMid]));
            joints.Add(JointType.SpineBase, getJointPositions(body.Joints[JointType.SpineBase]));
            joints.Add(JointType.SpineShoulder, getJointPositions(body.Joints[JointType.SpineShoulder]));

            joints.Add(JointType.ElbowLeft, getJointPositions(body.Joints[JointType.ElbowLeft]));
            joints.Add(JointType.ElbowRight, getJointPositions(body.Joints[JointType.ElbowRight]));

            joints.Add(JointType.WristLeft, getJointPositions(body.Joints[JointType.WristLeft]));
            joints.Add(JointType.WristRight, getJointPositions(body.Joints[JointType.WristRight]));

            joints.Add(JointType.HipLeft, getJointPositions(body.Joints[JointType.HipLeft]));
            joints.Add(JointType.HipRight, getJointPositions(body.Joints[JointType.HipRight]));

            joints.Add(JointType.KneeLeft, getJointPositions(body.Joints[JointType.KneeLeft]));
            joints.Add(JointType.KneeRight, getJointPositions(body.Joints[JointType.KneeRight]));

            joints.Add(JointType.AnkleLeft, getJointPositions(body.Joints[JointType.AnkleLeft]));
            joints.Add(JointType.AnkleRight, getJointPositions(body.Joints[JointType.AnkleRight]));

            joints.Add(JointType.FootLeft, getJointPositions(body.Joints[JointType.FootLeft]));
            joints.Add(JointType.FootRight, getJointPositions(body.Joints[JointType.FootRight]));
            
        }

        public static double GetAngle(BodyJoints body, JointType jointType1, JointType jointType2, JointType jointType3)
        {
            var joint1 = body.getJoint(jointType1);
            var joint2 = body.getJoint(jointType2);
            var joint3 = body.getJoint(jointType3);
            return joint2.Angle(joint1, joint3);
        }

    }
}
