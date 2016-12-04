using System.ComponentModel;
using System;
using Microsoft.Kinect;
using System.Timers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using Fleck;
using System.Collections.Generic;
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
        double scaleRatio = 1.0;
        string exerciseName = "";
        static List<IWebSocketConnection> _sockets;
        static bool _initialized = false;

        private bool bodyHasNotDeviated = true;
        //Joint Positions for the Ideal body
        BodyJoints idealBodyJoints;
        private String deviatedJointName = "";
        private JointType deviatedJointNumber = JointType.Head;

        bool positionChanged = true;

        public ViewModel(IKinectService kinectService)
        {
            dbManager = new DBManager();
            //dbManager.initializeDatabase();
            initializeTimer();
            InitializeSocketconnection();
            
            this.kinectService = kinectService;
            this.kinectService.SkeletonUpdated += new System.EventHandler<SkeletonEventArgs>(kinectService_SkeletonUpdated);
        }

        private void InitializeSocketconnection()
        {
            _sockets = new List<IWebSocketConnection>();

            var server = new WebSocketServer("ws://192.168.0.11:8181");

            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    _sockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    positionNumber = 1;
                    exerciseName = "";
                    _sockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    exerciseName = message;
                    positionNumber = 1;
                    positionChanged = true;
                    Console.WriteLine(message);
                    LoadPositionFile();
                };
            });

            _initialized = true;
        }

        private void sendMessageToClient(string message)
        {
            if (!_initialized)
                return;
            foreach (var socket in _sockets)
            {
                socket.Send(message);
            }
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
            idealJoints.Add(JObject.FromObject(idealBodyJoints.getJoint(JointType.SpineShoulder)));
            idealJoints.Add(JObject.FromObject(idealBodyJoints.getJoint(JointType.SpineBase)));

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
            trackedJoints.Add(JObject.FromObject(trackedBodyJoints.getJoint(JointType.SpineShoulder)));
            trackedJoints.Add(JObject.FromObject(trackedBodyJoints.getJoint(JointType.SpineBase)));

            JObject bodyJointsJson = new JObject();
            if (positionChanged)
            {
                bodyJointsJson.Add("idealBodyJoints", idealJoints);
                positionChanged = false;
            }
            bodyJointsJson.Add("trackedBodyJoints", trackedJoints);
            bodyJointsJson.Add("scaleRatio", scaleRatio);
            if (!String.IsNullOrEmpty(deviatedJointName))
            {
                bodyJointsJson.Add("deviatedJointName", deviatedJointName);
                bodyJointsJson.Add("deviatedJointNumber", (int)(deviatedJointNumber));
            }
            foreach (var socket in _sockets)
            {
                socket.Send(bodyJointsJson.ToString());
            }
        }

        private void LoadPositionFile(BodyJoints trackedBodyJoints = null)
        {
            Console.WriteLine("ExerciseName: " + exerciseName);
            if (String.IsNullOrEmpty(exerciseName))
                return;
            IFormatter formatter = new BinaryFormatter();
            string fileName = "..\\..\\..\\Dataset\\" + "sample" + "\\" + positionNumber.ToString() + ".bin";
            Console.WriteLine("FileName: " + fileName);
            if (File.Exists(fileName))
            {
                Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                BodyJoints fileObject = (BodyJoints)formatter.Deserialize(stream);
                stream.Close();
                idealBodyJoints = fileObject;
            }
            else
            {
                if (exerciseName.Equals("calibrate"))
                {
                    if (trackedBodyJoints != null)
                    {
                        double idealHeight = Math.Abs(idealBodyJoints.getJoint(JointType.Head).Position.Y - idealBodyJoints.getJoint(JointType.FootLeft).Position.Y);
                        double trackedHeight = Math.Abs(trackedBodyJoints.getJoint(JointType.Head).Position.Y - trackedBodyJoints.getJoint(JointType.FootLeft).Position.Y);
                        double idealWidth = Math.Abs(idealBodyJoints.getJoint(JointType.WristLeft).Position.X - idealBodyJoints.getJoint(JointType.WristRight).Position.X);
                        double trackedWidth = Math.Abs(trackedBodyJoints.getJoint(JointType.WristLeft).Position.X - trackedBodyJoints.getJoint(JointType.WristRight).Position.X);
                        scaleRatio = ((idealHeight / trackedHeight) + (idealWidth / trackedWidth)) / 2;
                    }
                }
                exerciseName = "";
                positionNumber = 1;
                idealBodyJoints = null;
                sendMessageToClient("You are done with the exercise");
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
            Stream stream = new FileStream("..\\..\\..\\Dataset\\sample\\" + i.ToString() + ".bin", FileMode.Create, FileAccess.Write, FileShare.None);
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
                    //Joint Positions for the Tracked body
                    BodyJoints trackedBodyJoints = new BodyJoints(trackedBody);
                    getSkeletonAndUpdateTimer(trackedBodyJoints);

                    if (idealBodyJoints == null)
                        return;
                    //Sending Tracked skeleton via web socket 
                    startSendingTrackedSkeleton(trackedBodyJoints);

                    //Compare the tracked and ideal right hand joint positions
                    setBodyDeviation(trackedBodyJoints, idealBodyJoints);
                }
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
        private void getSkeletonAndUpdateTimer(BodyJoints trackedBodyJoints)
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
                    LoadPositionFile(trackedBodyJoints);
                    stopwatch.Stop();
                    stopwatch.Reset();
                    positionChanged = true;
                }
            }
        }

        private void setBodyDeviation(BodyJoints trackedBodyJoints, BodyJoints idealBodyJoints)
        {
            if(idealBodyJoints != null)
            {
                trackedBodyJoints = TranslateSkeleton(idealBodyJoints, trackedBodyJoints);
                double totalDeviation = 0.0;
                double threshold = 80.0;
                totalDeviation = trackedBodyJoints - idealBodyJoints;
                Console.WriteLine("Total Deviation = " + totalDeviation + "Position: " + positionNumber);
                if (totalDeviation > threshold)
                {
                    Console.WriteLine(BodyJoints.maxDeviatedJoint.Item1 + " deviated by " + BodyJoints.maxDeviatedJoint.Item2);
                    deviatedJointName = BodyJoints.maxDeviatedJoint.Item1.ToString();
                    deviatedJointNumber = BodyJoints.maxDeviatedJoint.Item1;
                    bodyHasNotDeviated = false;
                }
                else
                {
                    deviatedJointName = "";
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