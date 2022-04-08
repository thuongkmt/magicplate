using KonbiBrain.WindowService.FacialRecognition.Services;
using KonbiBrain.WindowService.FacialRecognition.Util;
using Newtonsoft.Json;
using rsid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace KonbiBrain.WindowService.FacialRecognition
{
    public class RealSenseID
    {

        private static Authenticator _auth = new Authenticator();
        public List<(Faceprints, string)> faceprintsArray = new List<(Faceprints, string)>();//(faceprint,username)
        private static readonly Database _db = new Database();
        public string user_id = "";
        string comport = "";
        string securityLevel = "";
        string algoFlow = "";
        string faceSelectionPolicy = "";
        string cameraRotation = "";
        string dumpMode = "";
        public int faceprintsScore = 0;
        public int faceprintsConfidence = 0;
        public int faceprintsSuccess = 0;
        public string faceprintUserId = "";
        public int isCountDetect = 0;
        public bool isFaceDetected = false;
        public event EventHandler detectedFace;
        public string onAuthResult = "";
        public AuthStatus onAuthResultStatus;
        private RealsenseService _realsenseService;
        private int countForbidden = 0;

        public RealSenseID(string comport)
        {
            this.comport = comport;



        }
        /**
         * - HOST DEVICE MODE (we are using this mode)
         * **/
        public void EnrollExtractFaceprints()
        {
            if (!ConnectAuth())
            {
                Helper.WriteToFile($"[EnrollExtractFaceprints] Error connecting to device at port: {comport}");
            }
            var enrollExtractArgs = new EnrollExtractArgs
            {
                resultClbk = OnEnrollExtractionResult,
                progressClbk = OnEnrollProgress,
                hintClbk = OnEnrollHint,
                faceDetectedClbk = OnFaceDeteced
            };
            var status = _auth.EnrollExtractFaceprints(enrollExtractArgs);

            Helper.WriteToFile($" ==== END EnrollExtractFaceprints status ===:{status}");
        }
        public bool AuthenticateExtractFaceprints()
        {
            if (!ConnectAuth())
            {
                Helper.WriteToFile($"[AuthenticateExtractFaceprints] Error connecting to device at port: {comport}");
            }
            var _authExtFacePrintArgs = new AuthExtractArgs
            {
                resultClbk = OnAuthExtractionResult,
                hintClbk = OnAuthHint,
                faceDetectedClbk = OnFaceDeteced
            };
            var status = _auth.AuthenticateExtractFaceprints(_authExtFacePrintArgs);
            Helper.WriteToFile($"AuthenticateExtractFaceprints status: {status}");
            bool statusCheck = false;
            if (status == Status.Ok)
            {
                statusCheck = true;
            }
            return statusCheck;
        }
        public DeviceConfig QueryDeviceConfig()
        {

            Helper.WriteToFile("");
            Helper.WriteToFile("Query device config..");

            DeviceConfig deviceConfig = new DeviceConfig();
            var rv = _auth.QueryDeviceConfig(out deviceConfig);
            if (rv != Status.Ok)
            {
                Helper.WriteToFile("Query error: " + rv.ToString());
                return deviceConfig;

            }
            LogDeviceConfig(deviceConfig);
            return deviceConfig;

        }
        private void LogDeviceConfig(DeviceConfig deviceConfig)
        {
            Helper.WriteToFile(" - - -  Start to Device Config  - - - ");
            Helper.WriteToFile(" * Camera Rotation: " + deviceConfig.cameraRotation.ToString());
            Helper.WriteToFile(" * AntiSpoof Level: " + deviceConfig.securityLevel.ToString());
            Helper.WriteToFile(" * Algo Flow: " + deviceConfig.algoFlow);
            Helper.WriteToFile(" * Face Selection Policy: " + deviceConfig.faceSelectionPolicy);
            Helper.WriteToFile(" * Dump Mode: " + deviceConfig.dumpMode.ToString());
            //Helper.WriteToFile(" * Matcher Confidence Level(Not Accurate): " + deviceConfig.matcherConfidenceLevel.ToString());
            Helper.WriteToFile(" - - -  End to Device Config  - - - ");
        }
        public bool AuthenticateLoopExtractFaceprints()
        {
            bool statusCheck = false;
            try
            {
                if (!ConnectAuth())
                {
                    Helper.WriteToFile($"[AuthenticateLoopExtractFaceprints] Error connecting to device at port: {comport}");
                    detectedFace?.Invoke(this, new FaceprintEventArgs(AuthStatus.Failure));
                    return statusCheck;
                }
                var _authExtFacePrintArgs = new AuthExtractArgs
                {
                    resultClbk = OnAuthExtractionResult,
                    hintClbk = OnAuthHint,
                    faceDetectedClbk = OnFaceDeteced
                };
                var status = _auth.AuthenticateLoopExtractFaceprints(_authExtFacePrintArgs);

                switch (status)
                {
                    case Status.Ok:
                        statusCheck = true;
                        break;
                    case Status.SerialError:
                        Helper.WriteToFile($"[AuthenticateLoopExtractFaceprints] - End session");
                        break;
                    default:
                        Helper.WriteToFile($"[AuthenticateLoopExtractFaceprints] - End with status {status}");
                        break;
                }
                _auth.Disconnect();
                return statusCheck;
            }
            catch (Exception ex)
            {
                Helper.WriteToFile($"[AuthenticateExtractFaceprints] exception: {ex.ToString()}");
                return statusCheck;
            }
        }
        /**
         *  - LOCAL DEVICE MODE
         * **/
        public void Enroll()
        {
            if (_auth.Connect(new SerialConfig { port = comport }) != Status.Ok)
            {
                Helper.WriteToFile("Error connecting to device:");
            }
            rsid.DeviceConfig? deviceConfigForLogging = QueryDeviceConfig();
            var enrollArgs = new EnrollArgs
            {
                userId = user_id,
                resultClbk = OnEnrollResult,
                progressClbk = OnEnrollProgress,
                hintClbk = OnEnrollHint,
                faceDetectedClbk = OnFaceDeteced
            };
            var data = _auth.Enroll(enrollArgs);
            Helper.WriteToFile($"Enroll status {data}");
        }
        public void Authenticate()
        {
            if (_auth.Connect(new SerialConfig { port = comport }) != Status.Ok)
            {
                Helper.WriteToFile("Error connecting to device:");
            }
            DeviceConfig deviceConfigForLogging = QueryDeviceConfig();
            DeviceConfig deviceConfigFromSetting = new DeviceConfig();
            //Dictionary<string,DeviceConfig.SecurityLevel> securityDict = new Dictionary<string, DeviceConfig.SecurityLevel>()
            //{
            //   {"High",DeviceConfig.SecurityLevel.High},
            //   { "Medium",DeviceConfig.SecurityLevel.Medium},
            //   {"Low",DeviceConfig.SecurityLevel.Low},
            //};
            //Dictionary< string,DeviceConfig.AlgoFlow > algoFlowDict = new Dictionary<string, DeviceConfig.AlgoFlow>()
            //{
            //   {"All",DeviceConfig.AlgoFlow.All},
            //   {"FaceDetectionOnly",DeviceConfig.AlgoFlow.FaceDetectionOnly},
            //   {"RecognitionOnly",DeviceConfig.AlgoFlow.RecognitionOnly},
            //   {"SpoofOnly",DeviceConfig.AlgoFlow.SpoofOnly},
            //};
            //Dictionary< string, DeviceConfig.FaceSelectionPolicy> faceSelectionPolicyDict = new Dictionary<string, DeviceConfig.FaceSelectionPolicy>()
            //{
            //   {"All",DeviceConfig.FaceSelectionPolicy.All},
            //   {"Single",DeviceConfig.FaceSelectionPolicy.Single},
            //};
            //Dictionary<string,DeviceConfig.DumpMode> dumpModeDict = new Dictionary<string, DeviceConfig.DumpMode>()
            //{
            //   {"None",DeviceConfig.DumpMode.None},
            //   {"CroppedFace",DeviceConfig.DumpMode.CroppedFace},
            //   {"FullFrame",DeviceConfig.DumpMode.FullFrame},

            //};
            //Dictionary<string, DeviceConfig.CameraRotation> cameraRotationDict = new Dictionary<string, DeviceConfig.CameraRotation>()
            //{
            //   {"Rotation_0_Deg",DeviceConfig.CameraRotation.Rotation_0_Deg},
            //   {"Rotation_90_Deg",DeviceConfig.CameraRotation.Rotation_90_Deg},
            //   {"Rotation_180_Deg",DeviceConfig.CameraRotation.Rotation_180_Deg},
            //   {"Rotation_270_Deg",DeviceConfig.CameraRotation.Rotation_270_Deg},

            //};
            //try
            //{
            //    deviceConfigFromSetting.securityLevel = securityDict[securityLevel];
            //    deviceConfigFromSetting.algoFlow = algoFlowDict[algoFlow];
            //    deviceConfigFromSetting.faceSelectionPolicy = faceSelectionPolicyDict[faceSelectionPolicy];
            //    deviceConfigFromSetting.dumpMode = dumpModeDict[dumpMode];
            //    deviceConfigFromSetting.cameraRotation = cameraRotationDict[cameraRotation];
            //}
            //catch (Exception ex) {
            //    Helper.WriteToFile("deviceConfig from Config File failed:\n" + ex);
            //}

            //TODO: QTCHANGE force change the deviceconfig to below

            if (deviceConfigForLogging.faceSelectionPolicy!= DeviceConfig.FaceSelectionPolicy.Single || deviceConfigForLogging.securityLevel != DeviceConfig.SecurityLevel.Low || deviceConfigForLogging.dumpMode != DeviceConfig.DumpMode.None || deviceConfigForLogging.algoFlow != DeviceConfig.AlgoFlow.All|| deviceConfigForLogging.cameraRotation != DeviceConfig.CameraRotation.Rotation_0_Deg) {
                deviceConfigFromSetting.securityLevel = DeviceConfig.SecurityLevel.Low;
                deviceConfigFromSetting.algoFlow = DeviceConfig.AlgoFlow.All;
                deviceConfigFromSetting.faceSelectionPolicy = DeviceConfig.FaceSelectionPolicy.Single;
                deviceConfigFromSetting.dumpMode = DeviceConfig.DumpMode.None;
                deviceConfigFromSetting.cameraRotation = DeviceConfig.CameraRotation.Rotation_0_Deg;
                var status2 = _auth.SetDeviceConfig(deviceConfigFromSetting);
                if (status2 != Status.Ok)
                {
                    Helper.WriteToFile("SetDeviceConfig failed with error " + status2);
                }
                Helper.WriteToFile("[DeviceConfig] Config Updated! " + status2);
                DeviceConfig? forLogging = QueryDeviceConfig();

            }

            var _authArgs = new AuthArgs
            {
                hintClbk = OnAuthHint,
                resultClbk = OnAuthResult,
                faceDetectedClbk = OnFaceDeteced
            };
            var status = _auth.Authenticate(_authArgs);
            Helper.WriteToFile($"Authenticate status {status}");
        }
        //TODO: QTCHANGE add to setdeviceconfig

        public Status SetDeviceConfig(DeviceConfig args)
        {
            var status = _auth.SetDeviceConfig(args);


            return status;
        }
        /************************************************************************
         *  - CALLBACK FUNCTION
         * **********************************************************************/

        /**
         * Enrollment callback
         * **/
        private void OnEnrollProgress(FacePose status, IntPtr ctx)
        {
            Helper.WriteToFile($"[OnEnrollProgress] status {status}");
        }
        private void OnEnrollHint(EnrollStatus status, IntPtr ctx)
        {
            Helper.WriteToFile($"[OnEnrollHint] status {status}");
        }
        private void OnEnrollResult(EnrollStatus status, IntPtr ctx)
        {
            Helper.WriteToFile($"-------------------------OnEnroll {status}");
        }
        private void OnEnrollExtractionResult(EnrollStatus status, IntPtr faceprintsHandle, IntPtr ctx)
        {
            Helper.WriteToFile($"-------------------------OnEnrollExtractionResult status: {status}");
            if (status == EnrollStatus.Success)
            {
                var faceprints = (Faceprints)Marshal.PtrToStructure(faceprintsHandle, typeof(Faceprints));
                //add faceprint to file database
                if (_db.Push(faceprints, user_id))
                {
                    _db.Save();
                }
            }
        }
        /**
         * Authentication callbacks
         * **/
        private void OnAuthHint(AuthStatus hint, IntPtr ctx)
        {
            //Helper.WriteToFile("[OnAuthHint] status " + hint);
            if (AuthStatus.Failure == hint)
            {
                detectedFace?.Invoke(this, new FaceprintEventArgs(AuthStatus.Failure));
            }
        }
        private void OnAuthResult(AuthStatus status, string userId, IntPtr ctx)
        {
            Helper.WriteToFile("[OnAuthResult] " + status);
            onAuthResultStatus = status;
            if (status == AuthStatus.Success)
            {
                onAuthResult = userId;
                Helper.WriteToFile($"[OnAuthResult] Authenticated userId: {userId}");
            }
        }
        public void OnAuthExtractionResult(AuthStatus status, IntPtr faceprintsHandle, IntPtr ctx)
        {
            Helper.WriteToFile($"[OnAuthExtractionResult] status: {status}");
            isCountDetect++;
            Helper.WriteToFile($"[OnAuthExtractionResult] isCountDetect: {isCountDetect}");
            if (isCountDetect > 23)//timeout to loop detect
            {
                Standby();
                Helper.WriteToFile($"[OnAuthExtractionResult] Standby");
                isCountDetect = 0;
            }
            if (status == AuthStatus.Success)
            {
                var faceprints = (rsid.ExtractedFaceprints)Marshal.PtrToStructure(faceprintsHandle, typeof(rsid.ExtractedFaceprints));
                Match(faceprints);
            }
            else if (status == AuthStatus.Failure)
            {
                detectedFace?.Invoke(this, new FaceprintEventArgs(AuthStatus.Failure));
            }
        }
        /**
         * Common callback
         * **/
        private void OnFaceDeteced(IntPtr faceData, int faceCount, uint ts, IntPtr ctx)
        {
            Helper.WriteToFile($"[OnFaceDeteced] - faceCount: {faceCount} face(s)");
            //convert to face rects
            /*var faces = Authenticator.MarshalFaces(faceData, faceCount);
            foreach (var face in faces)
            {
                Helper.WriteToFile($"[OnFaceDeteced] facex,facey,withxheight: {face.x},{face.y},{face.width}x{face.height}");
            }*/
        }


        private void VerifyResult(bool result, string successMessage, string failMessage, Action onSuccess = null)
        {
            if (result)
            {
                Helper.WriteToFile(successMessage);
                onSuccess?.Invoke();
            }
            else
            {
                Helper.WriteToFile(failMessage);
            }
        }

        private bool Match(ExtractedFaceprints faceprintsToMatch)
        {
            //TODO: QTCHANGE to adapt new DLL

            Helper.WriteToFile("[Match] - Start to match faceprint");
            try
            {

                rsid.MatchElement faceprintsToMatchObject = new rsid.MatchElement
                {
                    version = faceprintsToMatch.version,
                    flags = faceprintsToMatch.featuresVector[rsid.FaceprintsConsts.RSID_INDEX_IN_FEATURES_VECTOR_TO_FLAGS],
                    featuresVector = faceprintsToMatch.featuresVector
                };

                int saveMaxScore = -1;
                int winningIndex = -1;
                string winningIdStr = "";
                var check = false;

                rsid.MatchResult winningMatchResult = new rsid.MatchResult { success = 0, shouldUpdate = 0, score = 0 };
                rsid.Faceprints winningUpdatedFaceprints = new rsid.Faceprints { }; // dummy init, correct data is set below if condition met.

                int usersIndex = 0;


                foreach (var (faceprintsDb, userIdDb) in faceprintsArray)
                {
                    // note we must send initialized vectors to MatchFaceprintsToFaceprints().
                    // so here we init the updated vector to the existing DB vector before calling MatchFaceprintsToFaceprints()
                    MatchArgs matchArgs = new MatchArgs
                    {
                        newFaceprints = faceprintsToMatchObject,
                        existingFaceprints = faceprintsDb,
                        updatedFaceprints = faceprintsDb, // init updated to existing vector.
                        matcherConfidenceLevel = MatcherConfidenceLevel.Low
                    };

                    var matchResult = _auth.MatchFaceprintsToFaceprints(ref matchArgs);

                    int currentScore = matchResult.score;

                    // save the best winner that matched.
                    if (matchResult.success == 1)
                    {
                        check = true;

                        if (currentScore > saveMaxScore)
                        {
                            saveMaxScore = currentScore;
                            winningMatchResult = matchResult;
                            winningIndex = usersIndex;
                            winningIdStr = userIdDb;
                            winningUpdatedFaceprints = matchArgs.updatedFaceprints;
                            Helper.WriteToFile($"Match info : userIdName = \"{winningIdStr}\", index = \"{winningIndex}\", success = {winningMatchResult.success}, score = {winningMatchResult.score}, should_update = {winningMatchResult.shouldUpdate}.");
                            if (winningMatchResult.shouldUpdate > 0)
                            {
                                // apply adaptive update
                                // take the updated vector from the matchArgs that were sent by reference and updated 
                                // during call to MatchFaceprintsToFaceprints() .

                                bool updateSuccess = UpdateUser(winningIndex, winningIdStr, ref winningUpdatedFaceprints);
                                Helper.WriteToFile($"Adaptive DB update for userIdName = \"{winningIdStr}\" (index=\"{winningIndex}\"): status = {updateSuccess} ");

                            }
                            else
                            {
                                Helper.WriteToFile($"Match succeeded for userIdName = \"{winningIdStr}\" (index=\"{winningIndex}\"). However adaptive update condition not passed, so no DB update applied.");

                            }
                            //log data
                            Helper.WriteToFile($"[Match] faceprints-score: {matchResult.score}");
                            Helper.WriteToFile($"[Match] faceprints-user_id: {userIdDb}");

                            faceprintsConfidence = matchResult.score;
                            faceprintsSuccess = matchResult.success;
                            faceprintUserId = userIdDb;
                            //PUB EVENT
                            detectedFace?.Invoke(this, new FaceprintEventArgs(AuthStatus.Success));
                            //standby immediately
                            Standby();
                            //reset timeout loop authen
                            isCountDetect = 0;
                            return true;
                        }

                    } // end of for() loop
                    usersIndex++;

                }
                //loop countForbidden
                countForbidden++;
                Helper.WriteToFile($"[Match] Not matched faceprint found - {AuthStatus.Forbidden} time {countForbidden}");
                if (countForbidden == 2)
                {
                    countForbidden = 0;
                    detectedFace?.Invoke(this, new FaceprintEventArgs(AuthStatus.Forbidden));
                    Helper.WriteToFile($"[Match] Not matched faceprint found - {AuthStatus.Forbidden} Invoke");
                }

                return check;
            }

            catch (Exception ex)
            {
                Helper.WriteToFile($"[Match] Exception {ex.Message}");
                return false;

            }
        }

        /************************************************************************
         *  - DATABASE PROCESSING
         * **********************************************************************/
        /************************************************************************
         *  - DATABASE PROCESSING
         * **********************************************************************/
        private bool UpdateUser(int userIndex, string userId, ref Faceprints updatedFaceprints)
        {
            //bool success = _db.UpdateUser(userIndex, userId, ref updatedFaceprints);

            //if (success)
            //{
            //    _db.Save();
            //    Helper.WriteToFile($"[Info] Update user {userId}'s faceprint to DB successful");
            //}
            return true;


            //string faceprintString = JsonConvert.SerializeObject(updatedFaceprints);
            //string faceprintEncode = Helper.Base64Encode(faceprintString);
            //var faceprintEnrollReq = new EnrollFaceprintRequest(
            //        userId,
            //        faceprintEncode,
            //        true
            //    );
            //string statusCode = "";
            //bool enrollStatus = false;

            //enrollStatus = _realsenseService.EnrollFaceprint(faceprintEnrollReq, out statusCode);
            //if (statusCode == "0")
            //{
            //    Helper.WriteToFile($"[ALERT] Call api to server timeout");
            //    return false;
            //}
            //else if (statusCode == "Unauthorized")
            //{

            //    Helper.WriteToFile($"[ALERT] Unauthorized");
            //    return false;
            //}
            //if (enrollStatus)
            //{
            //    Helper.WriteToFile($"[Enroll] - Update user {userId}'s faceprint to server successful");
            //    List<UserFaceprints> data = _auth.GetUsersFaceprints();
            //    var faceprintsArrayNew = new List<(Faceprints, string)>();
            //    foreach (var uf in data)
            //    {
            //        //get data from device then cache into the RAM
            //        faceprintsArrayNew.Add((uf.faceprints, uf.userID));
            //    }

            //    faceprintsArrayNew.Add((updatedFaceprints, userId));
            //    var checkImport = ImportFaceprintToDevice(faceprintsArrayNew);
            //    if (checkImport)
            //    {
            //        Helper.WriteToFile($"[Update] - Update user {userId}'s faceprint local successful");
            //    }
            //    else {
            //        Helper.WriteToFile($"[ALERT] - Update user {userId}'s faceprint local failed");
            //    }
            //}
            //if (success && enrollStatus)
            //{
            //    return true;
            //}
            //else {
            //    return false;
            //}


        }
        public bool ImportFaceprintToDevice(List<(Faceprints, string)> faceprintsArray)
        {
            bool checkImport = false;
            if (!ConnectAuth())
            {
                return checkImport;
            }
            try
            {
                if (WriteDataIntoDevice(faceprintsArray))
                {
                    checkImport = true;
                    Helper.WriteToFile($"[ImportFaceprintToDevice] Import faceprints done {faceprintsArray.Count()}");
                }
                else
                {
                    checkImport = false;
                    Helper.WriteToFile("[ImportFaceprintToDevice] Import faceprints failed");
                }
            }
            catch (Exception ex)
            {
                Helper.WriteToFile($"[ImportFaceprintToDevice] Import faceprints exception {ex}");
            }
            _auth.Disconnect();
            return checkImport;
        }
        public bool WriteDataIntoDevice(List<(Faceprints, string)> faceprints)
        {
            List<UserFaceprints> users = new List<UserFaceprints>();

            foreach (var (faceprintsDb, userIdDb) in faceprints)
            {
                var uf = new UserFaceprints();
                uf.userID = userIdDb;
                uf.faceprints = faceprintsDb;
                users.Add(uf);
            }
            bool check = false;
            if (_auth.SetUsersFaceprints(users))
            {
                check = true;
            }
            return check;
        }
        public bool RemoveUserFromDevice(string userId)
        {
            if (!ConnectAuth())
            {
                return false;
            }
            bool checkDelete = false;
            try
            {
                var status = _auth.RemoveUser(userId);
                if (status == Status.Ok)
                {
                    checkDelete = true;
                    Helper.WriteToFile($"[RemoveUserFromDevice] Delete user OK for user: {userId}");
                }
                else
                {
                    checkDelete = false;
                    Helper.WriteToFile($"[RemoveUserFromDevice] Delete user FAILED for user: {userId}");
                }
            }
            catch (Exception ex)
            {
                Helper.WriteToFile($"[RemoveUserFromDevice] Exception: {ex.Message}");
            }
            _auth.Disconnect();
            return checkDelete;
        }
        public bool CheckFaceprintOnRAM(string newUserId)
        {
            foreach (var (faceprintsLocal, userIdLocal) in faceprintsArray)
            {
                if (newUserId == userIdLocal)
                {
                    return true;
                }
            }
            return false;
        }
        public List<(Faceprints, string)> GetUserFaceprintFromDevice()
        {
            Helper.WriteToFile($"[GetUserFaceprintFromDevice] Start at {this.comport}");
            if (!ConnectAuth())
            {
                Helper.WriteToFile($"[GetUserFaceprintFromDevice] Error connecting to device at port: {this.comport}");
                return null;
            }
            try
            {
                faceprintsArray.Clear();
                var data = _auth.GetUsersFaceprints();
                //Helper.WriteToFile($"GetUserFaceprintFromDevice data at {JsonConvert.SerializeObject(data)}");
                foreach (var uf in data)
                {
                    //get data from device then cache into the RAM
                    faceprintsArray.Add((uf.faceprints, uf.userID));
                }
                Helper.WriteToFile($"[GetUserFaceprintFromDevice] FACEPRINT LOAD FROM REALSENSE_ID DEVICE: {faceprintsArray.Count}");
            }
            catch (Exception ex)
            {
                Helper.WriteToFile($"[GetUserFaceprintFromDevice] Exception {ex.StackTrace}");
            }

            _auth.Disconnect();

            return faceprintsArray;
        }
        public Status RemoveAllUserInDevice()
        {
            if (!ConnectAuth())
            {
                return Status.SerialError;
            }
            Status status = Status.Error;
            try
            {
                status = _auth.RemoveAllUsers();
            }
            catch (Exception ex)
            {
                Helper.WriteToFile($"[RemoveAllUserInDevice] Exception: {ex.Message}");
            }
            finally
            {
                _auth.Disconnect();
            }
            return status;
        }
        private bool ConnectAuth()
        {
            var comport = new SerialConfig
            {
                port = this.comport
            };
            var status = _auth.Connect(comport);
            if (status != Status.Ok)
            {
                Helper.WriteToFile($"Connection Failed to Port {this.comport}");
                return false;
            }
            return true;
        }
        public void Standby()
        {
            if (!ConnectAuth())
                return;
            try
            {
                _auth.Standby();
            }
            catch (Exception ex)
            {
                Helper.WriteToFile($"[Standby] exception  {ex.Message}");
            }
        }
    }
    public class FaceprintEventArgs : EventArgs
    {
        public AuthStatus status { get; set; }
        public FaceprintEventArgs(AuthStatus status)
        {
            this.status = status;
        }
    }
}
