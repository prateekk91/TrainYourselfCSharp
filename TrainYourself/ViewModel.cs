using System.ComponentModel;
using System;
using Microsoft.Kinect;
using System.Timers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Windows.Input;
using System.Windows;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Fleck;
using System.Collections.Generic;
using Coding4Fun.Kinect.Wpf;
using LightBuzz.Vitruvius;

namespace KinectMvvm
{

    [Serializable]
    [JsonConverter(typeof(SkeletonSerializer))]
    public class BodyJoints
    {
        Dictionary<JointType, Position> joints;

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

            return joint;
        }

        public static double operator-(BodyJoints body1, BodyJoints body2)
        {
            double totalDeviation = 0;
            var shoulder = body1.getJoint(JointType.ShoulderRight);
            var elbow = body1.getJoint(JointType.ElbowRight);
            var hand = body1.getJoint(JointType.WristRight);

            var shoulder2 = body2.getJoint(JointType.ShoulderRight);
            var elbow2 = body2.getJoint(JointType.ElbowRight);
            var hand2 = body2.getJoint(JointType.WristRight);

            var angle1 = elbow.Angle(shoulder, hand);
            var angle2 = elbow2.Angle(shoulder2, hand2);
            totalDeviation = angle1;
            return totalDeviation;
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
            joints.Add( JointType.Head , getJointPositions(body.Joints[JointType.Head]));
            
            joints.Add( JointType.ShoulderRight , getJointPositions(body.Joints[JointType.ShoulderRight]));
            joints.Add( JointType.ShoulderLeft , getJointPositions(body.Joints[JointType.ShoulderLeft]));
            
            joints.Add( JointType.SpineMid , getJointPositions(body.Joints[JointType.SpineMid]));
            joints.Add( JointType.SpineBase , getJointPositions(body.Joints[JointType.SpineBase]));
            
            joints.Add( JointType.ElbowLeft , getJointPositions(body.Joints[JointType.ElbowLeft]));
            joints.Add( JointType.ElbowRight , getJointPositions(body.Joints[JointType.ElbowRight]));

            joints.Add(JointType.WristLeft, getJointPositions(body.Joints[JointType.WristLeft]));
            joints.Add( JointType.WristRight , getJointPositions(body.Joints[JointType.WristRight]));
        
            joints.Add( JointType.HipLeft , getJointPositions(body.Joints[JointType.HipLeft]));
            joints.Add( JointType.HipRight , getJointPositions(body.Joints[JointType.HipRight]));

            joints.Add( JointType.KneeLeft , getJointPositions(body.Joints[JointType.KneeLeft]));
            joints.Add(JointType.KneeRight, getJointPositions(body.Joints[JointType.KneeRight]));
            
            joints.Add( JointType.AnkleLeft, getJointPositions(body.Joints[JointType.AnkleLeft]));
            joints.Add(JointType.AnkleRight, getJointPositions(body.Joints[JointType.AnkleRight]));

            joints.Add(JointType.FootLeft, getJointPositions(body.Joints[JointType.FootLeft]));
            joints.Add(JointType.FootRight, getJointPositions(body.Joints[JointType.FootRight]));
        }

    }

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

    public class ViewModel : INotifyPropertyChanged
    {
        IKinectService kinectService;
        static List<IWebSocketConnection> _clients = new List<IWebSocketConnection>();
        
        private ICommand _saveCommand;

        public ICommand SaveCommand
        {
            get
            {
                if (_saveCommand == null)
                {
                    _saveCommand = new RelayCommand(
                        param => this.SaveObject(),
                        param => this.CanSave()
                    );
                }
                return _saveCommand;
            }
        }

        private bool CanSave()
        {
            // Verify command can be executed here
            return true;
        }

        private void SaveObject()
        {
            SerializeSkeleton();
        }

        //Database Manager and Timer declaration
        DBManager dbManager;
        Timer timer;

        private bool bodyHasNotDeviated = true;
        //Joint Positions for the Ideal body
        BodyJoints idealBodyJoints;

        public ViewModel(IKinectService kinectService)
        {
            dbManager = new DBManager();
            //dbManager.initializeDatabase();
            initializeTimer();
            
            this.kinectService = kinectService;
            this.kinectService.SkeletonUpdated += new System.EventHandler<SkeletonEventArgs>(kinectService_SkeletonUpdated);


            //BodyJoints fileObject = new BodyJoints(idealBodyJoints);
            //IFormatter formatter = new BinaryFormatter();
            //Stream stream = new FileStream("F:\\College\\Quarter4\\218\\project\\positions\\exercise1\\1.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            //formatter.Serialize(stream, fileObject);
            //stream.Close();

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("..\\..\\..\\Dataset\\1.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
            BodyJoints fileObject = (BodyJoints)formatter.Deserialize(stream);
            stream.Close();
            idealBodyJoints = fileObject;

            //IFormatter formatter2 = new BinaryFormatter();
            //Stream stream2 = new FileStream("F:\\College\\Quarter4\\218\\project\\positions\\exercise1\\1.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
            //BodyJoints fileObject2 = (BodyJoints)formatter.Deserialize(stream);
            //stream.Close();
            //float[,] idealBodyJoints2 = fileObject.idealBodyJoints;
            /*
            float[,] idealBodyJoints2 = {{ -0.009447459f , 0.60294944f, 1.31400883f} ,
                                                    { 0.002266258f, 0.42004475f, 1.32388508f }  ,
                                                    { 0.000935936f , 0.213289276f, 1.30896533f } ,
                                                    {  0.000739973853f, -0.0607283078f, 1.25822461f }  ,
                                                    { 0.220867485f, -0.1269243f, 1.17617548f },
                                                    { -0.1677815f, -0.136166364f, 1.1603055f },
                                                    { 0.07320645f, -0.0546527356f   ,1.22442615f },
                                                    { -0.07176963f, -0.06349322f, 1.22342634f },
                                                    { 0.238081276f, -0.330413043f, 1.10450733f },
                                                    { -0.114430159f, -0.391302735f, 1.197041f },
                                                    { 0.121732004f, -0.8049696f, 1.131731f },
                                                    { 0.0298294257f, -0.8586146f, 1.19491315f },
                                                    { 0.000113591552f, -0.7418386f, 1.09284163f },
                                                    { 0.1216219f, -0.822920859f, 1.08214772f }};
            //this.idealBodyJoints = idealBodyJoints2;
            //setBodyDeviation(idealBodyJoints, idealBodyJoints2);
            */

            //****************************************************
            /*
            
            string jsonBodyJoints = JsonConvert.SerializeObject(fileObject3);

            Console.WriteLine("printing json " + jsonBodyJoints);

            var server = new WebSocketServer("ws://127.0.0.1:8181");
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    _clients.Add(socket);
                    socket.Send(jsonBodyJoints);
                };

                socket.OnClose = () =>
                {
                    _clients.Remove(socket);
                };

                socket.OnMessage = message =>
                {
                    foreach (var client in _clients)
                    {
                        // Send the message to everyone!
                        // Also, send the client connection's unique identifier in order
                        // to recognize who is who.
                        //       client.Send(client.ConnectionInfo.Id + " says: " + message);
                        client.Send("sending body joints");

                    }
                    Console.WriteLine("Switched to " + message);
                };
            });

            */
            //***************************************************8
        }

        private void initializeTimer()
        {
            if (timer == null)
                timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(TimerElapsed);
            timer.Interval = 50;
            timer.Enabled = true;
        }

        static int i = 1;

        public void SerializeSkeleton()
        {
            BodyJoints fileObject = new BodyJoints(this.trackedBody);
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("..\\..\\..\\Dataset\\exercise2\\" + i.ToString() + ".bin", FileMode.Create, FileAccess.Write, FileShare.None);
            i++;
            formatter.Serialize(stream, fileObject);
            stream.Close();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            //dbManager.setData("Skeletons are not the same");
        }

        void kinectService_SkeletonUpdated(object sender, SkeletonEventArgs e)
        {
            if (App.Current.MainWindow != null)
            {
                this.trackedBody = e.TrackedBody;
                getSkeletonAndUpdateTimer();

                //Joint Positions for the Tracked body
                BodyJoints trackedBodyJoints = new BodyJoints(trackedBody);
                
                //Compare the tracked and ideal right hand joint positions
                setBodyDeviation(trackedBodyJoints, idealBodyJoints);
            }
        }
        
        Body trackedBody;
        public Body TrackedBody
        {
            get { return this.trackedBody; }
            set
            {
                this.trackedBody = value;
                this.OnPropertyChanged("TrackedBody");
            }
        }

        void OnPropertyChanged(string property)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public void Cleanup()
        {
            this.kinectService.SkeletonUpdated -= kinectService_SkeletonUpdated;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //Skeleton processing code
        private void getSkeletonAndUpdateTimer()
        {
            if (bodyHasNotDeviated)
            {
                timer.Enabled = false;
                timer.Interval = 50;
                //dbManager.setData("Skeletons are the same");
                Console.WriteLine("Skeletons are the same");
                timer.Enabled = true;
            }
        }
        
        private void setBodyDeviation(BodyJoints trackedBodyJoints, BodyJoints idealBodyJoints)
        {
            trackedBodyJoints = TranslateSkeleton(idealBodyJoints, trackedBodyJoints);
            double totalDeviation = 0.0;
            double threshold = 500.0;
            totalDeviation = trackedBodyJoints - idealBodyJoints;
            Console.WriteLine("Total Deviation = " + totalDeviation);
            if (totalDeviation > threshold)
            {
                Console.WriteLine("too much error detected");
                bodyHasNotDeviated = false;
            }
            else
            {
                bodyHasNotDeviated = true;
            }
        }

        private BodyJoints TranslateSkeleton(BodyJoints idealBody, BodyJoints trackedBody)
        {
            int numJoints = idealBody.getNumJoints();
            
            Joint idealHeadJoint = idealBody.getJoint(JointType.Head);
            float idealHeadX = idealHeadJoint.Position.X;
            float idealHeadY = idealHeadJoint.Position.Y;
            float idealHeadZ = idealHeadJoint.Position.Z;

            Joint trackedHeadJoint = trackedBody.getJoint(JointType.Head);
            float trackedHeadX = trackedHeadJoint.Position.X;
            float trackedHeadY = trackedHeadJoint.Position.Y;
            float trackedHeadZ = trackedHeadJoint.Position.Z;

            float[] diff = { idealHeadX - trackedHeadX, idealHeadY - trackedHeadY, idealHeadZ - trackedHeadZ };
            
            trackedBody.Translate(diff);

            return trackedBody;
        }
    }
}