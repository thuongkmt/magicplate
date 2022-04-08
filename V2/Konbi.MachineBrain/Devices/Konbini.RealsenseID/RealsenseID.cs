using Newtonsoft.Json;
using rsid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Konbini.RealsenseID
{
    public class RealsenseID
    {
        private static Authenticator _auth = new Authenticator();
        public List<(Faceprints, string)> faceprintsArray = new List<(Faceprints, string)>();
        public string user_id = "";
        string comport = "";
        public int faceprintsScore = 0;
        public int faceprintsConfidence = 0;
        public int faceprintsSuccess = 0;
        public string faceprintUserId = "";

        public RealsenseID(string comport)
        {
            this.comport = comport;
        }
        /**
         * - HOST DEVICE MODE (we are using this mode)
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
            if (_auth.Connect(new SerialConfig { port = comport }) != Status.Ok)
            {
                WriteToFile("RealSenseID: Error connecting to device:");
            }
            var _authExtFacePrintArgs = new AuthExtractArgs
            {
                resultClbk = OnAuthExtractionResult,
                hintClbk = OnAuthHint,
                faceDetectedClbk = OnFaceDeteced
            };
            var status = _auth.AuthenticateExtractFaceprints(_authExtFacePrintArgs);
            WriteToFile($"AuthenticateExtractFaceprints status: {status}");
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
         *  - LOCAL DEVICE MODE
         * **/
        public void Enroll()
        {
            if (_auth.Connect(new SerialConfig { port = comport }) != Status.Ok)
            {
                System.Console.WriteLine("Error connecting to device:");
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
            Console.WriteLine($"Enroll status {data}");
        }
        public void Authenticate()
        {
            if (_auth.Connect(new SerialConfig { port = comport }) != Status.Ok)
            {
                System.Console.WriteLine("Error connecting to device:");
            }
            var _authArgs = new AuthArgs
            {
                hintClbk = OnAuthHint,
                resultClbk = OnAuthResult,
                faceDetectedClbk = OnFaceDeteced
            };
            var status = _auth.Authenticate(_authArgs);
            Console.WriteLine($"Authenticate status {status}");
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
            Console.WriteLine($"-------------------------OnEnrollExtractionResult status: {status}");
            if (status == EnrollStatus.Success)
            {
                var faceprints = (Faceprints)Marshal.PtrToStructure(faceprintsHandle, typeof(Faceprints));
                //add faceprint to file database
                /*if (_db.Push(faceprints, user_id))
                {
                    _db.Save();
                }*/
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
            Console.WriteLine("OnResults " + status);
            if (status == AuthStatus.Success)
            {
                Console.WriteLine("Authenticated " + userId);
            }
        }
        private void OnAuthExtractionResult(AuthStatus status, IntPtr faceprintsHandle, IntPtr ctx)
        {
            WriteToFile($" -------------------------OnAuthExtractionResult status: {status}");
            if (status == AuthStatus.Success)
            {
                var faceprints = (Faceprints)Marshal.PtrToStructure(faceprintsHandle, typeof(Faceprints));
                var strFaceprint = JsonConvert.SerializeObject(faceprints);
                WriteToFile($"-------------------------OnAuthExtractionResult faceprints: {strFaceprint}");
                Match(faceprints);
            }
        }
        /**
         * Common callback
         * **/
        private void OnFaceDeteced(IntPtr faceData, int faceCount, uint ts, IntPtr ctx)
        {
            Console.WriteLine($"---------OnFaceDeteced: {faceCount} face(s)");
            WriteToFile($"---------OnFaceDeteced: {faceCount} face(s)");
            //convert to face rects
            var faces = Authenticator.MarshalFaces(faceData, faceCount);
            foreach (var face in faces)
            {
                Console.WriteLine($"--------- OnFaceDeteced {face.x},{face.y}, {face.width}x{face.height}");
                WriteToFile($"--------- OnFaceDeteced {face.x},{face.y}, {face.width}x{face.height}");
            }
        }
        private void Match(Faceprints faceprintsToMatch)
        {
            /*try
            {
                // if Faceprints versions don't match - return with error message.
                if (!(_db.VerifyVersionMatched(ref faceprintsToMatch)))
                {
                    string logmsg = $"Faceprints (FP) version mismatch: DB={ _db.GetVersion()}, FP={faceprintsToMatch.version}. Saved the old DB to backup file and started a new DB from scratch.";
                    string guimsg = $"Faceprints (FP) version mismatch: DB={ _db.GetVersion()}, FP={faceprintsToMatch.version}. DB backuped and cleaned.";

                    WriteToFile(logmsg);
                    WriteToFile(guimsg);
                    return;
                }
                faceprintsScore = 0;
                faceprintsConfidence = 0;
                faceprintsSuccess = 0;
                faceprintUserId = "";
                // TODO yossidan - handle with/without mask vectors properly (if/as needed).

                foreach (var (faceprintsDb, userIdDb) in _db.faceprintsArray)
                {
                    // note we must send initialized vectors to MatchFaceprintsToFaceprints().
                    // so here we init the updated vector to the existing DB vector before calling MatchFaceprintsToFaceprints()
                    MatchArgs matchArgs = new MatchArgs
                    {
                        newFaceprints = faceprintsToMatch,
                        existingFaceprints = faceprintsDb,
                        updatedFaceprints = faceprintsDb // init updated to existing vector.
                    };

                    // TODO yossidan - handle with/without mask vectors properly (if/as needed).

                    var matchResult = _auth.MatchFaceprintsToFaceprints(ref matchArgs);
                    var userIndex = 0;
                    if (matchResult.success == 1)
                    {
                        // update the DB with the updated faceprints.
                        if (matchResult.shouldUpdate > 0)
                        {
                            // take the updated vector from the matchArgs that were sent by reference and updated 
                            // during call to MatchFaceprintsToFaceprints() .

                            bool update_success = UpdateUser(userIndex, userIdDb, ref matchArgs.updatedFaceprints);
                        }
                        else
                        {
                            Console.WriteLine($"Macth succeeded for user \"{userIdDb}\". However adaptive update condition not passed, so no DB update applied.");
                            WriteToFile($"Macth succeeded for user \"{userIdDb}\". However adaptive update condition not passed, so no DB update applied.");
                        }

                        //log data
                        //show result here
                        Console.WriteLine($"-------------------------Match faceprints-score: {matchResult.score}");
                        Console.WriteLine($"-------------------------Match faceprints-confidence: {matchResult.confidence}");
                        Console.WriteLine($"-------------------------Match faceprints-success: {matchResult.success}");
                        Console.WriteLine($"-------------------------Match faceprints-shouldUpdate: {matchResult.shouldUpdate}");
                        Console.WriteLine($"-------------------------Match faceprints-user_id: {userIdDb}");

                        WriteToFile($"-------------------------Match faceprints-score: {matchResult.score}");
                        WriteToFile($"-------------------------Match faceprints-confidence: {matchResult.confidence}");
                        WriteToFile($"-------------------------Match faceprints-success: {matchResult.success}");
                        WriteToFile($"-------------------------Match faceprints-shouldUpdate: {matchResult.shouldUpdate}");
                        WriteToFile($"-------------------------Match faceprints-user_id: {userIdDb}");

                        faceprintsScore = matchResult.score;
                        faceprintsConfidence = matchResult.confidence;
                        faceprintsSuccess = matchResult.success;
                        faceprintUserId = userIdDb;
                        return;
                    }
                    userIndex++;
                }
                Console.WriteLine($"{AuthStatus.Forbidden}, string.Empty, No match found");
                WriteToFile($"{AuthStatus.Forbidden}, string.Empty, No match found");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
                WriteToFile($"{ex.Message}");
            }*/
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
        public void ImportFaceprintToDevice()
        {
            var faceprintsArray = new List<(Faceprints, string)>(); // ==> get from_external_resource exmple from api
            try
            {
                if (WriteDataIntoDevice(faceprintsArray))
                    Console.WriteLine("----------- import done");
                else
                {
                    Console.WriteLine("----------- import failed");
                }
            }
            catch (Exception ex)
            {
                //
            }
            finally
            {
                //
            }
        }
        public bool WriteDataIntoDevice(List<(Faceprints, string)> faceprint)
        {
            List<UserFaceprints> users = new List<UserFaceprints>();

            foreach (var (faceprintsDb, userIdDb) in faceprintsArray)
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
        //get data from device then cache into the RAM
        public List<(Faceprints, string)> GetUserFaceprintFromDevice()
        {
            if (!ConnectAuth())
            {
                return null;
            }
            try
            {
                var data = _auth.GetUsersFaceprints();
                foreach (var uf in data)
                {
                    faceprintsArray.Add((uf.faceprints, uf.userID));
                }
                WriteToFile($"FACPRINT ARRAY LOAD FROM REALSENSEID DEVICE {faceprintsArray.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                WriteToFile($"Exception {ex.Message}");
            }

            _auth.Disconnect();

            return faceprintsArray;
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
                WriteToFile($"Connection Failed to Port {this.comport}");
                return false;
            }
            WriteToFile($"Connection succeeded to Port {this.comport}");
            return true;
        }
        /**
         * 
         * System processing:Write log into file
         * 
         * **/
        public void WriteToFile(string Message)
        {
            string path = @"C:\Logs";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string filepath = @"C:\Logs\RealsenseID_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";

            DateTime saveNow = DateTime.Now;
            DateTime localTime;
            localTime = saveNow.ToLocalTime();

            if (!File.Exists(filepath))
            {
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine("[" + localTime + "] - " + Message);
                }
            }

            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine("[" + localTime + "] - " + Message);
                }
            }
        }
    }
}
