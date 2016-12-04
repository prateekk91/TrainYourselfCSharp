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

            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.Head)));    //0
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.Neck)));    //1
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.ShoulderRight)));   //2
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.ShoulderLeft)));    //3
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.SpineMid)));        //4
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.ElbowLeft)));       //5
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.ElbowRight)));      //6
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.WristLeft)));       //7
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.WristRight)));      //8
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.HipLeft)));         //9
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.HipRight)));        //10
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.KneeLeft)));        //11
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.KneeRight)));       //12
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.FootLeft)));        //13
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.FootRight)));       //14
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.SpineShoulder)));   //15
            joints.Add(JObject.FromObject(bodyJoints.getJoint(JointType.SpineBase)));       //16

            JObject jointJson = new JObject();
            jointJson.Add("joints", joints);

            jointJson.WriteTo(writer);
        }
    }
}
