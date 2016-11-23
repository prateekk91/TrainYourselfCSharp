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
using System.Diagnostics;

namespace KinectMvvm
{
    
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
        Stopwatch stopwatch = new Stopwatch();
        int positionNumber = 1;
        static List<IWebSocketConnection> _sockets;
        static bool _initialized = false;

        private bool bodyHasNotDeviated = true;
        //Joint Positions for the Ideal body
        BodyJoints idealBodyJoints;

        public ViewModel(IKinectService kinectService)
        {
            dbManager = new DBManager();
            //dbManager.initializeDatabase();
            initializeTimer();
            InitializeSocketconnection();
            
            this.kinectService = kinectService;
            this.kinectService.SkeletonUpdated += new System.EventHandler<SkeletonEventArgs>(kinectService_SkeletonUpdated);


            //BodyJoints fileObject = new BodyJoints(idealBodyJoints);
            //IFormatter formatter = new BinaryFormatter();
            //Stream stream = new FileStream("F:\\College\\Quarter4\\218\\project\\positions\\exercise1\\1.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            //formatter.Serialize(stream, fileObject);
            //stream.Close();

            LoadPositionFile();

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
        }

        private void InitializeSocketconnection()
        {
            _sockets = new List<IWebSocketConnection>();

            var server = new WebSocketServer("ws://127.0.0.1:8181");

            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    _sockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    _sockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    Console.WriteLine(message);
                };
            });

            _initialized = true;
        }

        private void startSendingTrackedSkeleton(BodyJoints trackedBodyJoints)
        {
            if (!_initialized) return;

            JArray idealJoints = new JArray();
            idealJoints.Add(JObject.FromObject(idealBodyJoints.getJoint(JointType.Head)));
            idealJoints.Add(JObject.FromObject(idealBodyJoints.getJoint(JointType.Neck)));
            idealJoints.Add(JObject.FromObject(idealBodyJoints.getJoint(JointType.ShoulderRight)));
            idealJoints.Add(JObject.FromObject(idealBodyJoints.getJoint(JointType.ShoulderLeft)));
            idealJoints.Add(JObject.FromObject(idealBodyJoints.getJoint(JointType.SpineMid)));
            idealJoints.Add(JObject.FromObject(idealBodyJoints.getJoint(JointType.ElbowLeft)));
            idealJoints.Add(JObject.FromObject(idealBodyJoints.getJoint(JointType.ElbowRight)));
            idealJoints.Add(JObject.FromObject(idealBodyJoints.getJoint(JointType.WristLeft)));
            idealJoints.Add(JObject.FromObject(idealBodyJoints.getJoint(JointType.WristRight)));
            idealJoints.Add(JObject.FromObject(idealBodyJoints.getJoint(JointType.HipLeft)));
            idealJoints.Add(JObject.FromObject(idealBodyJoints.getJoint(JointType.HipRight)));
            idealJoints.Add(JObject.FromObject(idealBodyJoints.getJoint(JointType.KneeLeft)));
            idealJoints.Add(JObject.FromObject(idealBodyJoints.getJoint(JointType.KneeRight)));
            idealJoints.Add(JObject.FromObject(idealBodyJoints.getJoint(JointType.FootLeft)));
            idealJoints.Add(JObject.FromObject(idealBodyJoints.getJoint(JointType.FootRight)));

            JArray trackedJoints = new JArray();
            trackedJoints.Add(JObject.FromObject(trackedBodyJoints.getJoint(JointType.Head)));
            trackedJoints.Add(JObject.FromObject(trackedBodyJoints.getJoint(JointType.Neck)));
            trackedJoints.Add(JObject.FromObject(trackedBodyJoints.getJoint(JointType.ShoulderRight)));
            trackedJoints.Add(JObject.FromObject(trackedBodyJoints.getJoint(JointType.ShoulderLeft)));
            trackedJoints.Add(JObject.FromObject(trackedBodyJoints.getJoint(JointType.SpineMid)));
            trackedJoints.Add(JObject.FromObject(trackedBodyJoints.getJoint(JointType.ElbowLeft)));
            trackedJoints.Add(JObject.FromObject(trackedBodyJoints.getJoint(JointType.ElbowRight)));
            trackedJoints.Add(JObject.FromObject(trackedBodyJoints.getJoint(JointType.WristLeft)));
            trackedJoints.Add(JObject.FromObject(trackedBodyJoints.getJoint(JointType.WristRight)));
            trackedJoints.Add(JObject.FromObject(trackedBodyJoints.getJoint(JointType.HipLeft)));
            trackedJoints.Add(JObject.FromObject(trackedBodyJoints.getJoint(JointType.HipRight)));
            trackedJoints.Add(JObject.FromObject(trackedBodyJoints.getJoint(JointType.KneeLeft)));
            trackedJoints.Add(JObject.FromObject(trackedBodyJoints.getJoint(JointType.KneeRight)));
            trackedJoints.Add(JObject.FromObject(trackedBodyJoints.getJoint(JointType.FootLeft)));
            trackedJoints.Add(JObject.FromObject(trackedBodyJoints.getJoint(JointType.FootRight)));

            JObject bodyJointsJson = new JObject();
            bodyJointsJson.Add("idealBodyJoints", idealJoints);
            bodyJointsJson.Add("trackedBodyJoints", trackedJoints);

            Console.WriteLine("printing json " + bodyJointsJson);

            foreach (var socket in _sockets)
            {
                socket.Send(bodyJointsJson.ToString());
            }
        }

        private void LoadPositionFile()
        {
            IFormatter formatter = new BinaryFormatter();
            string fileName = "..\\..\\..\\Dataset\\exercise1\\" + positionNumber.ToString() + ".bin";
            if (File.Exists(fileName))
            {
                Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                BodyJoints fileObject = (BodyJoints)formatter.Deserialize(stream);
                stream.Close();
                idealBodyJoints = fileObject;
            }
            else
            {
                idealBodyJoints = null;
                Console.WriteLine("You are done with the exercise");
            }
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
            Stream stream = new FileStream("..\\..\\..\\Dataset\\exercise1\\" + i.ToString() + ".bin", FileMode.Create, FileAccess.Write, FileShare.None);
            i++;
            formatter.Serialize(stream, fileObject);
            stream.Close();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            //Console.WriteLine("Skeleton not being detected");
            //dbManager.setData("Skeletons are not the same");
        }

        void kinectService_SkeletonUpdated(object sender, SkeletonEventArgs e)
        {
            if (App.Current.MainWindow != null)
            {
                this.trackedBody = e.TrackedBody;

                if (idealBodyJoints != null)
                {
                    getSkeletonAndUpdateTimer();
                }

                //Joint Positions for the Tracked body
                BodyJoints trackedBodyJoints = new BodyJoints(trackedBody);

                //Sending Tracked skeleton via web socket 
                startSendingTrackedSkeleton(trackedBodyJoints);

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

                if(stopwatch.Elapsed.TotalMilliseconds >= 2000)
                {
                    positionNumber += 1;
                    Console.WriteLine("Next file loaded for position: " + positionNumber);
                    LoadPositionFile();
                    stopwatch.Stop();
                    stopwatch.Reset();
                }
            }
        }

        private void setBodyDeviation(BodyJoints trackedBodyJoints, BodyJoints idealBodyJoints)
        {
            if(idealBodyJoints != null)
            {
                trackedBodyJoints = TranslateSkeleton(idealBodyJoints, trackedBodyJoints);
                double totalDeviation = 0.0;
                double threshold = 50.0;
                totalDeviation = trackedBodyJoints - idealBodyJoints;
                Console.WriteLine("Total Deviation = " + totalDeviation + "Position: " + positionNumber);
                if (totalDeviation > threshold)
                {
                    Console.WriteLine(BodyJoints.maxDeviatedJoint.Item1 + " deviated by " + BodyJoints.maxDeviatedJoint.Item2);
                    bodyHasNotDeviated = false;
                }
                else
                {
                    bodyHasNotDeviated = true;
                    if (!stopwatch.IsRunning)
                    {
                        stopwatch.Start();
                    }
                }
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