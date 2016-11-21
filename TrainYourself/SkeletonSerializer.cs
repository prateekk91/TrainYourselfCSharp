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
            JObject jsonObj = new JObject();

            BodyJoints bodyJoints = (BodyJoints)value;

            JArray joints = new JArray();
            //for (int i = 0; i < bodyJoints.idealBodyJoints.GetLength(0); i++)
            //{
            //    JObject joint = new JObject();
            //    joint.Add("x", bodyJoints.idealBodyJoints[i, 0]);
            //    joint.Add("y", bodyJoints.idealBodyJoints[i, 1]);
            //    joint.Add("z", bodyJoints.idealBodyJoints[i, 2]);
            //    joints.Add(joint);
            //}


            JObject jointJson = new JObject();
            jointJson.Add("joints", joints);

            jointJson.WriteTo(writer);
        }
    }
}
