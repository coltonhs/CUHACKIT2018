/**
* Copyright 2015 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using UnityEngine;
using System.Collections;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.DataTypes;
using System.Collections.Generic;
using UnityEngine.UI;
using IBM.Watson.DeveloperCloud.Services.Conversation.v1;
using FullSerializer;
using IBM.Watson.DeveloperCloud.Connection;

public class SpeechSandboxStreaming : MonoBehaviour
{
	public weapon Weapon;
	public GunManager gun;
	public GameObject impactEffect;
	public GameObject door;
	public GameObject doorTarget;
	public float doorspeed;

	public Automatic auto;
	//public Shotgun shot;


    public GameManager gameManager;
    public AudioClip sorryClip;
    public List<AudioClip> helpClips; 
    
	public AudioClip intro;
	public AudioClip instructions;
	public AudioClip wrongDoor;
	public AudioClip reloading;
	public AudioClip ammoReloaded;
	public AudioClip switchingWeapons;
	private bool soundPlayed = false;
	public GameObject distraction;

    [SerializeField]
    private fsSerializer _serializer = new fsSerializer();
    private SpeechToText _speechToText;
    private Conversation _conversation;

	private string stt_username = "affd2711-d814-4fcd-b5da-a688e91ef2b8";
	private string stt_password = "Aco8FH5uhIqu";
    // Change stt_url if different from below
    private string stt_url = "https://stream.watsonplatform.net/speech-to-text/api";
     
	private string convo_username = "608e2955-8ba2-452b-9fc1-3701d92e2ce1";
		private string convo_password = "PcYMqL7UnBlQ";
    // Change convo_url if different from below
    private string convo_url = "https://gateway.watsonplatform.net/conversation/api";
    // Change  _conversationVersionDate if different from below
    private string _conversationVersionDate = "2017-05-26";
	private string convo_workspaceId = "d0be94c1-ad97-4a8d-a57b-683f8ab2f42c";
	private string _url = "https://stream.watsonplatform.net/speech-to-text/api";

    public Text ResultsField;
	public string ResultField;

    private int _recordingRoutine = 0;
    private string _microphoneID = null;
    private AudioClip _recording = null;
    private int _recordingBufferSize = 1;
    private int _recordingHZ = 22050;

   
    void Start()
    {
		soundPlayed = false;
        LogSystem.InstallDefaultReactors();
		gameManager.PlayClip (intro);


        //  Create credential and instantiate service
        Credentials stt_credentials = new Credentials(stt_username, stt_password, stt_url);
        Credentials convo_credentials = new Credentials(convo_username, convo_password, convo_url);

        _speechToText = new SpeechToText(stt_credentials);
        _conversation = new Conversation(convo_credentials);
        _conversation.VersionDate = _conversationVersionDate;
        Active = true;



        StartRecording();
    }

    public bool Active
    {
        get { return _speechToText.IsListening; }
        set
        {
            if (value && !_speechToText.IsListening)
            {
                _speechToText.DetectSilence = true;
                _speechToText.EnableWordConfidence = true;
                _speechToText.EnableTimestamps = true;
                _speechToText.SilenceThreshold = 0.01f;
                _speechToText.MaxAlternatives = 0;
                _speechToText.EnableInterimResults = true;
                _speechToText.OnError = OnError;
                _speechToText.InactivityTimeout = -1;
                _speechToText.ProfanityFilter = false;
                _speechToText.SmartFormatting = true;
                _speechToText.SpeakerLabels = false;
                _speechToText.WordAlternativesThreshold = null;
                _speechToText.StartListening(OnRecognize, OnRecognizeSpeaker);
            }
            else if (!value && _speechToText.IsListening)
            {
                _speechToText.StopListening();
            }
        }
    }

    private void StartRecording()
    {
        if (_recordingRoutine == 0)
        {
            UnityObjectUtil.StartDestroyQueue();
            _recordingRoutine = Runnable.Run(RecordingHandler());
        }
    }

    private void StopRecording()
    {
        if (_recordingRoutine != 0)
        {
            Microphone.End(_microphoneID);
            Runnable.Stop(_recordingRoutine);
            _recordingRoutine = 0;
        }
    }

    private void OnError(string error)
    {
        Active = false;

        Log.Debug("ExampleStreaming.OnError()", "Error! {0}", error);
    }

    private void OnFail(RESTConnector.Error error, Dictionary<string, object> customData)
    {
        Log.Error("ExampleConversation.OnFail()", "Error received: {0}", error.ToString());
    }

    private IEnumerator RecordingHandler()
    {
        Log.Debug("ExampleStreaming.RecordingHandler()", "devices: {0}", Microphone.devices);
        _recording = Microphone.Start(_microphoneID, true, _recordingBufferSize, _recordingHZ);
        yield return null;      // let _recordingRoutine get set..

        if (_recording == null)
        {
            StopRecording();
            yield break;
        }

        bool bFirstBlock = true;
        int midPoint = _recording.samples / 2;
        float[] samples = null;

        while (_recordingRoutine != 0 && _recording != null)
        {
            int writePos = Microphone.GetPosition(_microphoneID);
            if (writePos > _recording.samples || !Microphone.IsRecording(_microphoneID))
            {
                Log.Error("ExampleStreaming.RecordingHandler()", "Microphone disconnected.");

                StopRecording();
                yield break;
            }

            if ((bFirstBlock && writePos >= midPoint)
              || (!bFirstBlock && writePos < midPoint))
            {
                // front block is recorded, make a RecordClip and pass it onto our callback.
                samples = new float[midPoint];
                _recording.GetData(samples, bFirstBlock ? 0 : midPoint);

                AudioData record = new AudioData();
				record.MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(samples)), Mathf.Max(samples));
                record.Clip = AudioClip.Create("Recording", midPoint, _recording.channels, _recordingHZ, false);
                record.Clip.SetData(samples, 0);

                _speechToText.OnListen(record);

                bFirstBlock = !bFirstBlock;
            }
            else
            {
                // calculate the number of samples remaining until we ready for a block of audio, 
                // and wait that amount of time it will take to record.
                int remaining = bFirstBlock ? (midPoint - writePos) : (_recording.samples - writePos);
                float timeRemaining = (float)remaining / (float)_recordingHZ;

                yield return new WaitForSeconds(timeRemaining);
            }

        }

        yield break;
    }

    private void OnRecognize(SpeechRecognitionEvent result)
    {
        if (result != null && result.results.Length > 0)
        {
            foreach (var res in result.results)
            {
                foreach (var alt in res.alternatives)
                {
                    if (res.final && alt.confidence > 0)
                    {
                        string text = alt.transcript;
                        Debug.Log("Result: " + text + " Confidence: " + alt.confidence);
                        _conversation.Message(OnMessage, OnFail, convo_workspaceId, text);
                    }
                }

                if (res.keywords_result != null && res.keywords_result.keyword != null)
                {
                    foreach (var keyword in res.keywords_result.keyword)
                    {
                        Log.Debug("ExampleStreaming.OnRecognize()", "keyword: {0}, confidence: {1}, start time: {2}, end time: {3}", keyword.normalized_text, keyword.confidence, keyword.start_time, keyword.end_time);
                    }
                }

                if (res.word_alternatives != null)
                {
                    foreach (var wordAlternative in res.word_alternatives)
                    {
                        Log.Debug("ExampleStreaming.OnRecognize()", "Word alternatives found. Start time: {0} | EndTime: {1}", wordAlternative.start_time, wordAlternative.end_time);
                        foreach(var alternative in wordAlternative.alternatives)
                            Log.Debug("ExampleStreaming.OnRecognize()", "\t word: {0} | confidence: {1}", alternative.word, alternative.confidence);
                    }
                }
            }
        }
    }

    void OnMessage(object resp, Dictionary<string, object> customData)
    {
        //  Convert resp to fsdata

        fsData fsdata = null;
        fsResult r = _serializer.TrySerialize(resp.GetType(), resp, out fsdata);
        if (!r.Succeeded)
            throw new WatsonException(r.FormattedMessages);

        //  Convert fsdata to MessageResponse
        MessageResponse messageResponse = new MessageResponse();
        object obj = messageResponse;
        r = _serializer.TryDeserialize(fsdata, obj.GetType(), ref obj);
        if (!r.Succeeded)
            throw new WatsonException(r.FormattedMessages);

        if (resp != null && (messageResponse.intents.Length > 0 || messageResponse.entities.Length > 0))
        {
            string intent = messageResponse.intents[0].intent;
            Debug.Log("Intent: " + intent);
            string currentMat = null;
            string currentScale = null;



			if (intent == "open") {
				foreach (RuntimeEntity entity in messageResponse.entities) {
					Debug.Log ("entityType: " + entity.entity + " , value: " + entity.value);
					if (entity.entity == "door") {
						Debug.Log ("OPEN DOOR");
					}
				}
			} else if (intent == "instructions" && !soundPlayed) {
				gameManager.PlayClip (instructions);
			} else if (intent == "distraction") {
				Distraction ();

			} else if (intent == "ready") {
				gameManager.PlayClip (wrongDoor);
				//float step = doorspeed * Time.deltaTime;
				//door.transform.position = Vector3.MoveTowards (door.transform.position, doorTarget.transform.position, step);
				Destroy (door, .1f);
			}
				
			else if (intent == "reload") {
				Debug.Log ("RELOAD");

				if (gun.selectedWeapon) {
					if (gun.shot < 5) {
						//if (!soundPlayed) {
						gameManager.PlayClip (reloading);
						//soundPlayed = true;
						StartCoroutine (ReloadingShot ());
					}
				}
				if (!gun.selectedWeapon) {
					if (gun.assault < 15) {
						gameManager.PlayClip (reloading);
						StartCoroutine (ReloadingAuto ());

					}
				}
					Debug.Log ("RELOAD FINISHED");


				foreach (RuntimeEntity entity in messageResponse.entities) {
					if (entity.entity == "fire") {
						Debug.Log ("ENTITY.ENTITY is SHOOT");
					}
					if (entity.entity == "fire") {
						Debug.Log ("ENTITY.ENTITY is SHOOT");
					}
				}
				intent = null;
			}

			else if(intent == "switch") {
				//Switch weapons
				gun.selectedWeapon = !gun.selectedWeapon;
				gameManager.PlayClip (switchingWeapons);


				
        }
        else
        {
            Debug.Log("Failed to invoke OnMessage();");


				//gameManager.PlayClip (dontUnderstand);
				
			}

        }
	}

    private void OnRecognizeSpeaker(SpeakerRecognitionEvent result)
    {
        if (result != null)
        {
            foreach (SpeakerLabelsResult labelResult in result.speaker_labels)
            {
                Log.Debug("ExampleStreaming.OnRecognize()", string.Format("speaker result: {0} | confidence: {3} | from: {1} | to: {2}", labelResult.speaker, labelResult.from, labelResult.to, labelResult.confidence));
            }
        }
    }

	IEnumerator ReloadingShot() {


			
		yield return new WaitForSeconds(1.5f);
		gun.shot = 5;
		gameManager.PlayClip (ammoReloaded);
			
		
	}

	IEnumerator ReloadingAuto() {

		yield return new WaitForSeconds (1.5f);
		gun.assault = 15;
		gameManager.PlayClip (ammoReloaded);
	}


	public void Distraction()
	{
		GameObject impactGO = Instantiate (impactEffect) as GameObject;
		GameObject[] gos = GameObject.FindGameObjectsWithTag ("Zombie");

		foreach (GameObject go in gos) {
			go.GetComponent<Zombie> ().ChangeTarget (impactGO.transform);
		}
	}


}
