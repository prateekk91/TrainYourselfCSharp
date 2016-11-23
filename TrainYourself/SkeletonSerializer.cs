using Microsoft.Kinect;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace KinectMvvm
{
    public class SkeletonSerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(BodyJoints).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            BodyJoints bodyJoints = (BodyJoints)value;

            JArray joints = new JArray();

            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.Head)));
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.Neck)));
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.ShoulderRight)));
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.ShoulderLeft)));
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.SpineMid)));
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.ElbowLeft)));
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.ElbowRight)));
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.WristLeft)));
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.WristRight)));
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.HipLeft)));
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.HipRight)));
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.KneeLeft)));
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.KneeRight)));
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.FootLeft)));
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.FootRight)));

            JObject jointJson = new JObject();
            jointJson.Add("joints", joints);

            jointJson.WriteTo(writer);
        }
    }
}
