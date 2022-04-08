using Konbi.RealsenseID.Util;
using Newtonsoft.Json;
using rsid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Konbi.RealsenseID
{
    public class RealsenseID
    {
        public Authenticator _auth;
        public List<(Faceprints, string)> faceprintsArray = new List<(Faceprints, string)>();//(faceprint,username)
        public string user_id = "";
        private string comport = "";
        private int databaseVersion = 0;
        public int faceprintsScore = 0;
        public int faceprintsConfidence = 0;
        public int faceprintsSuccess = 0;
        public string faceprintUserId = "";
        public AuthStatus onAuthExtractionResult;
        public AuthStatus onAuthResult;
        public EnrollStatus onEnrollExtractStatus;
        public Faceprints currentEnrollFaceprint;
        public string userIdDetected = "";
        public bool cancelDevice = false;

        public RealsenseID(string comport, int databaseVersion)
        {
            try
            {
                this.comport = comport;
                this.databaseVersion = databaseVersion;
                _auth = new Authenticator();
            }
            catch (Exception ex)
            {
                Helper.WriteToFile(ex.ToString());
            }
        }
        /**
         * - HOST DEVICE MODE 
         * **/
        public void EnrollExtractFaceprints()
        {
            if (_auth.Connect(new SerialConfig { port = comport }) != Status.Ok)
            {
                System.Console.WriteLine("Error connecting to device:");
            }
            var enrollExtractArgs = new EnrollExtractArgs
            {
                resultClbk = OnEnrollExtractionResult,
                progressClbk = OnEnrollProgress,
                hintClbk = OnEnrollHint,
                faceDetectedClbk = OnFaceDeteced
            };
            var status = _auth.EnrollExtractFaceprints(enrollExtractArgs);

            Console.WriteLine($" ==== END EnrollExtractFaceprints status ===:{status}");
        }
        public bool AuthenticateExtractFaceprints()
        {
            if (!ConnectAuth())
            {
                Helper.WriteToFile("RealSenseID: Error connecting to device:");
                return false;
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
        public void AuthenticateLoopExtractFaceprints()
        {
            if (_auth.Connect(new SerialConfig { port = comport }) != Status.Ok)
            {
                System.Console.WriteLine("Error connecting to device:");
            }
            var _authExtFacePrintArgs = new AuthExtractArgs
            {
                resultClbk = OnAuthExtractionResult,
                hintClbk = OnAuthHint,
                faceDetectedClbk = OnFaceDeteced
            };
            var status = _auth.AuthenticateLoopExtractFaceprints(_authExtFacePrintArgs);
            Console.WriteLine($" ==== END AuthenticateExtractFaceprints status ===: {status}");
        }
        /**
         *  - LOCAL DEVICE MODE (we are using this mode)
         * **/
        public Status Enroll(string user_id)
        {
            if (!ConnectAuth())
            {
                return Status.SecurityError;
            }
            var enrollArgs = new EnrollArgs
            {
                userId = user_id,
                resultClbk = OnEnrollResult,
                progressClbk = OnEnrollProgress,
                hintClbk = OnEnrollHint,
                faceDetectedClbk = OnFaceDeteced
            };
            var data = _auth.Enroll(enrollArgs);
            return data;
        }
        public Status EnrollExtract()
        {
            if (!ConnectAuth())
            {
                return Status.SecurityError;
            }
            try
            {
                var enrollExtArgs = new EnrollExtractArgs
                {
                    hintClbk = OnEnrollHint,
                    resultClbk = OnEnrollExtractionResult,
                    progressClbk = OnEnrollProgress,
                    faceDetectedClbk = OnFaceDeteced,
                };
                var status = _auth.EnrollExtractFaceprints(enrollExtArgs);
                return status;
            }
            catch (Exception ex)
            {
                Helper.WriteToFile($"EnrollExtract exception: {ex.Message}");
                return Status.Error;
            }
            finally
            {
                _auth.Disconnect();
            }
        }
        public Status Authenticate()
        {
            Status status;
            if (!ConnectAuth())
            {
                status = Status.SerialError;
            }
            var _authArgs = new AuthArgs
            {
                hintClbk = OnAuthHint,
                resultClbk = OnAuthResult,
                faceDetectedClbk = OnFaceDeteced
            };
            status = _auth.Authenticate(_authArgs);
            return status;
        }
        public Status AuthenticateLoop()
        {
            return Status.Ok;
        }

        /**
         * Enroll a user using an image of his face.
         * @param[in] user_id Null terminated C string of ascii chars. Max user id size is MAX_USERID_LENGTH bytes
         * @param[in] buffer bgr24 image buffer of the enrolled user face. Max buffer size is 950MB(i.e. Width x Height x 3 should not exceed it)
         * @param[in] width image width.
         * @param[in] width image height.
         * @return EnrollStatus (EnrollStatus::Success on success).
         * **/
        public EnrollStatus EnrollImage(string userId, byte[] buffer, int width, int height)
        {
            if (!ConnectAuth())
            {
                return EnrollStatus.Serial_Error;
            }
            var status = _auth.EnrollImage(userId, buffer, width, height);
            return status;
        }
        /**
         * Cancel
         * **/
        public Status Cancel()
        {
            if (!ConnectAuth())
            {
                return Status.SerialError;
            }
            try
            {
                var status = _auth.Cancel();
                Thread.Sleep(500);
                return status;
            }
            catch (Exception ex)
            {
                return Status.Error;
            }
            finally
            {
                _auth.Disconnect();
            }
        }

        /************************************************************************
         *  - CALLBACK FUNCTION
         * **********************************************************************/

        /**
         * Enrollment callback
         * **/
        private void OnEnrollProgress(FacePose status, IntPtr ctx)
        {
            Console.WriteLine($"OnEnrollProgress {status}");
        }
        private void OnEnrollHint(EnrollStatus status, IntPtr ctx)
        {
            Console.WriteLine($"OnEnrollHint {status}");
        }
        private void OnEnrollResult(EnrollStatus status, IntPtr ctx)
        {
            Console.WriteLine($"-------------------------OnEnroll {status}");
        }
        private void OnEnrollExtractionResult(EnrollStatus status, IntPtr faceprintsHandle, IntPtr ctx)
        {
            if (cancelDevice)
            {
                Helper.WriteToFile($"---------OnEnrollExtractionResult: Device was cancled");
            }
            else
            {
                Console.WriteLine($"-------------------------OnEnrollExtractionResult status: {status}");
                onEnrollExtractStatus = status;
                if (status == EnrollStatus.Success)
                {
                    currentEnrollFaceprint = (Faceprints)Marshal.PtrToStructure(faceprintsHandle, typeof(Faceprints));
                    Helper.WriteToFile($"--------- OnEnrollExtractionResult -faceprint {JsonConvert.SerializeObject(currentEnrollFaceprint)}");
                }
            }
            
        }
        /**
         * Authentication callbacks
         * **/
        private void OnAuthHint(AuthStatus hint, IntPtr ctx)
        {
            Console.WriteLine("OnHint " + hint);
        }
        private void OnAuthResult(AuthStatus status, string userId, IntPtr ctx)
        {
            Helper.WriteToFile($"OnAuthResult status: {status}");
            onAuthResult = status;
            if (status == AuthStatus.Success)
            {
                userIdDetected = userId;
                Helper.WriteToFile($"OnAuthResult detected userID: {userId}");
                //get faceprint from userId
            }
        }
        private void OnAuthExtractionResult(AuthStatus status, IntPtr faceprintsHandle, IntPtr ctx)
        {
            if (cancelDevice)
            {
                Helper.WriteToFile($"---------OnAuthExtractionResult: Device was cancled");
            }
            {
                Helper.WriteToFile($" -------------------------OnAuthExtractionResult status: {status}");
                onAuthExtractionResult = status;
                if (status == AuthStatus.Success)
                {
                    var faceprints = (rsid.ExtractedFaceprints)Marshal.PtrToStructure(faceprintsHandle, typeof(rsid.ExtractedFaceprints));
                    var strFaceprint = JsonConvert.SerializeObject(faceprints);
                    Helper.WriteToFile($"-------------------------OnAuthExtractionResult faceprints: {strFaceprint}");
                    Match(faceprints);
                }
            }
            
        }
        private void OnFaceDeteced(IntPtr faceData, int faceCount, uint ts, IntPtr ctx)
        {
            if (cancelDevice)
            {
                Helper.WriteToFile($"---------OnFaceDeteced: Device was cancled");
            }
            else
            {
                Helper.WriteToFile($"---------OnFaceDeteced: {faceCount} face(s)");
                //convert to face rects
                /*var faces = Authenticator.MarshalFaces(faceData, faceCount);
                foreach (var face in faces)
                {
                    Helper.WriteToFile($"--------- OnFaceDeteced {face.x},{face.y}, {face.width}x{face.height}");
                }*/
            }
            
        }
        /**
         * Match face
         * **/
        private bool Match(ExtractedFaceprints faceprintsToMatch)
        {
            try
            {
                faceprintsScore = 0;
                faceprintsConfidence = 0;
                faceprintsSuccess = 0;
                faceprintUserId = "";
                // TODO yossidan - handle with/without mask vectors properly (if/as needed).
                var check = false;

                MatchElement faceprintsToMatchObject = new rsid.MatchElement
                {
                    version = faceprintsToMatch.version,
                    flags = faceprintsToMatch.featuresVector[rsid.FaceprintsConsts.RSID_INDEX_IN_FEATURES_VECTOR_TO_FLAGS],
                    featuresVector = faceprintsToMatch.featuresVector
                };

                foreach (var (faceprintLocal, userIdLocal) in faceprintsArray)
                {
                    // note we must send initialized vectors to MatchFaceprintsToFaceprints().
                    // so here we init the updated vector to the existing DB vector before calling MatchFaceprintsToFaceprints()
                    MatchArgs matchArgs = new MatchArgs
                    {
                        newFaceprints = faceprintsToMatchObject,
                        existingFaceprints = faceprintLocal,
                        updatedFaceprints = faceprintLocal // init updated to existing vector.
                    };

                    // TODO yossidan - handle with/without mask vectors properly (if/as needed).

                    var matchResult = _auth.MatchFaceprintsToFaceprints(ref matchArgs);
                    var userIndex = 0;
                    if (matchResult.success == 1)
                    {
                        check = true;
                        // update the DB with the updated faceprints.
                        if (matchResult.shouldUpdate > 0)
                        {
                            // take the updated vector from the matchArgs that were sent by reference and updated 
                            // during call to MatchFaceprintsToFaceprints() .

                            bool update_success = UpdateUser(userIndex, userIdLocal, ref matchArgs.updatedFaceprints);
                        }
                        else
                        {
                            Helper.WriteToFile($"Macth succeeded for user \"{userIdLocal}\". However adaptive update condition not passed, so no DB update applied.");
                        }
                        //log data
                        Helper.WriteToFile($"-------------------------Match faceprints-score: {matchResult.score}");
                        Helper.WriteToFile($"-------------------------Match faceprints-confidence: {matchResult.confidence}");
                        Helper.WriteToFile($"-------------------------Match faceprints-success: {matchResult.success}");
                        Helper.WriteToFile($"-------------------------Match faceprints-shouldUpdate: {matchResult.shouldUpdate}");
                        Helper.WriteToFile($"-------------------------Match faceprints-user_id: {userIdLocal}");

                        faceprintsScore = matchResult.score;
                        faceprintsConfidence = matchResult.confidence;
                        faceprintsSuccess = matchResult.success;
                        faceprintUserId = userIdLocal;
                        return true;
                    }
                    userIndex++;
                }
                Helper.WriteToFile($"No match found - {AuthStatus.Forbidden}");
                return check;
            }
            catch (Exception ex)
            {
                Helper.WriteToFile($"{ex.Message}");
                return false;
            }
        }

        /************************************************************************
         *  - DATABASE PROCESSING
         * **********************************************************************/
        private bool UpdateUser(int userIndex, string userId, ref Faceprints updatedFaceprints)
        {
            /*bool success = _db.UpdateUser(userIndex, userId, ref updatedFaceprints);

            if (success)
            {
                _db.Save();
            }*/

            return true;
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
                    Helper.WriteToFile($"Import faceprints done {faceprintsArray.Count()}");
                }
                else
                {
                    checkImport = false;
                    Helper.WriteToFile("Import faceprints finished");
                }
            }
            catch (Exception ex)
            {
                Helper.WriteToFile($"Import faceprints exception {ex}");
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
            //check data version
            //
            if (_auth.SetUsersFaceprints(users))
            {
                check = true;
            }
            _auth.Disconnect();
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
                    Helper.WriteToFile($"Delete user OK for user: {userId}");
                }
                else
                {
                    checkDelete = false;
                    Helper.WriteToFile($"Delete user FAILED for user: {userId}");
                }
            }
            catch (Exception ex)
            {
                Helper.WriteToFile($"Exception: {ex.Message}");
            }
            _auth.Disconnect();
            return checkDelete;
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
            catch(Exception ex)
            {
                Helper.WriteToFile($"[RemoveAllUserInDevice] Exception: {ex.Message}");
            }
            finally
            {
                _auth.Disconnect();
            }
            return status;
        }
        public bool CheckUserFromDeviceById(string newUserId)
        {
            foreach (var (faceprintsLocal, userIdLocal) in faceprintsArray)
            {
                if(newUserId == userIdLocal)
                {
                    return true;
                }
            }
            return false;
        }
        public List<(Faceprints, string)> GetUserFaceprintFromDevice()
        {
            Helper.WriteToFile($"Start GetUserFaceprintFromDevice at {this.comport}");
            if (!ConnectAuth())
            {
                Helper.WriteToFile($"ConnectAuth GetUserFaceprintFromDevice Falied at {this.comport}");
                return null;
            }
            try
            {
                Helper.WriteToFile($"[GetUserFaceprintFromDevice] try at {this.comport}");
                faceprintsArray.Clear();
                Helper.WriteToFile($"[GetUserFaceprintFromDevice] Clear");
                var data = _auth.GetUsersFaceprints();
                //Helper.WriteToFile($"GetUserFaceprintFromDevice data at {JsonConvert.SerializeObject(data)}");
                Helper.WriteToFile($"comhere");
                int count = 0;
                if (data != null)
                {
                    foreach (var uf in data)
                    {
                        count++;
                        //get data from device then cache into the RAM
                        faceprintsArray.Add((uf.faceprints, uf.userID));
                        if (uf.userID == "daniel" || uf.userID =="thuongkmt")
                        {
                            Helper.WriteToFile($"[GetUserFaceprintFromDevice] faceprint of {uf.userID} {JsonConvert.SerializeObject(uf.faceprints)}");
                        }
                    }
                    Helper.WriteToFile($"[GetUserFaceprintFromDevice] FACEPRINT LOAD FROM REALSENSE_ID DEVICE: {faceprintsArray.Count}");
                    
                }
                else
                {
                    faceprintsArray = null;
                }
                Helper.WriteToFile($"[GetUserFaceprintFromDevice] done");

            }
            catch (Exception ex)
            {
                Helper.WriteToFile($"Exception {ex.Message}");
            }

            _auth.Disconnect();

            return faceprintsArray;
        }
        public string[] GetAllUserIdFromDevice()
        {
            if (!ConnectAuth())
            {
                Helper.WriteToFile($"[GetAllUserIdFromDevice] connect port {this.comport} failed");
                return null;
            }
            string[] users;
            Status status = _auth.QueryUserIds(out users);
            if(status != Status.Ok)
            {
                Helper.WriteToFile($"[GetAllUserIdFromDevice] Query failed: {status} ");
                throw new Exception(" error: " + status.ToString());
            }
            _auth.Disconnect();
            return users;

        }
        public bool ConnectAuth()
        {
            try
            {
                Helper.WriteToFile($"Start to check connection to RealsenseID at {this.comport}");
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
                Helper.WriteToFile($"Connection succeeded to Port {this.comport}");
                return true;
            }
            catch(Exception ex)
            {
                Helper.WriteToFile($"Connection Exception at Port {ex.Message}");
                return false;
            }
        }
        public string Version()
        {
            return Authenticator.Version();
        }
    }
}
