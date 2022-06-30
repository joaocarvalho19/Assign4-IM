﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mmisharp;
using Microsoft.Speech.Recognition;
using System.Xml.Linq;
//using Newtonsoft.Json;

namespace speechModality
{
    public class SpeechMod
    {
        // changed 16 april 2020
        private static SpeechRecognitionEngine sre= new SpeechRecognitionEngine(new System.Globalization.CultureInfo("pt-PT"));
        private Grammar gr;

        // Flag to check new game reponse
        Boolean new_game_flag = false;

        // Flag to check hint reponse
        Boolean hint_flag = false;

        public event EventHandler<SpeechEventArg> Recognized;
        protected virtual void onRecognized(SpeechEventArg msg)
        {
            EventHandler<SpeechEventArg> handler = Recognized;
            if (handler != null)
            {
                handler(this, msg);
            }
        }

        private LifeCycleEvents lce;
        private MmiCommunication mmic;

        //  NEW 16 april
        private static Tts tts = new Tts(sre);
        private MmiCommunication mmiReceiver;

        public SpeechMod()
        {
            //init LifeCycleEvents..
            lce = new LifeCycleEvents("ASR", "FUSION", "speech-1", "acoustic", "command"); // LifeCycleEvents(string source, string target, string id, string medium, string mode)
            mmic = new MmiCommunication("localhost",9876,"User1", "ASR");  //PORT TO FUSION - uncomment this line to work with fusion later
            //mmic = new MmiCommunication("localhost", 8000, "User1", "ASR"); // MmiCommunication(string IMhost, int portIM, string UserOD, string thisModalityName)

            mmic.Send(lce.NewContextRequest());

            //load pt recognizer
            //sre = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("pt-PT"));
            gr = new Grammar(Environment.CurrentDirectory + "\\ptG.grxml", "rootRule");
            sre.LoadGrammar(gr);


            sre.SetInputToDefaultAudioDevice();
            sre.RecognizeAsync(RecognizeMode.Multiple);
            sre.SpeechRecognized += Sre_SpeechRecognized;
            sre.SpeechHypothesized += Sre_SpeechHypothesized;

            // NEW - TTS support 16 April
            tts.Speak("Olá. Estou pronta para receber ordens. Boa Sorte !");



            //  o TTS  no final indica que se recebe mensagens enviadas para TTS
            mmiReceiver = new MmiCommunication("localhost",8000, "User1", "TTS");
        mmiReceiver.Message += MmiReceived_Message;
        mmiReceiver.Start();


        }


    private void Sre_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            //tts.Speak("O seu pedido não é válido. Tente outra vez.");
            onRecognized(new SpeechEventArg() { Text = e.Result.Text, Confidence = e.Result.Confidence, Semantics = e.Result.Semantics.ToString(), Final = false });
        }

        //
        private void Sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            onRecognized(new SpeechEventArg() { Text = e.Result.Text, Confidence = e.Result.Confidence, Final = true });
            Console.WriteLine(e.Result.Confidence);

            if (e.Result.Confidence > 0.4)
            {
                //SEND
                // IMPORTANT TO KEEP THE FORMAT {"recognized":["SHAPE","COLOR"]}
                string json = "{ \"recognized\": [";
                foreach (var resultSemantic in e.Result.Semantics)
                {
                    string final_value = null;
                    // Check if is an answer to a new game ask after an abort
                    if (new_game_flag)
                    {
                        if (resultSemantic.Value.Value + "_" == "YES_")
                        {
                            final_value = "YES_NEWGAME";
                            Console.WriteLine(final_value);
                            json += "\"" + final_value + "\", ";
                        }
                        else
                        {
                            Console.WriteLine(resultSemantic);
                            json += "\"" + resultSemantic.Value.Value + "\", ";
                        }
                    }
                    else if (hint_flag) {
                        if (resultSemantic.Value.Value + "_" == "YES_")
                        {
                            final_value = "YES_CLUE";
                            Console.WriteLine(final_value);
                            json += "\"" + final_value + "\", ";
                        }
                        else
                        {
                            Console.WriteLine(resultSemantic);
                            json += "\"" + resultSemantic.Value.Value + "\", ";
                        }
                    }
                    else
                    {
                        Console.WriteLine(resultSemantic);
                        json += "\"" + resultSemantic.Value.Value + "\", ";
                    }
                }

                new_game_flag = false;
                hint_flag = false;
                json = json.Substring(0, json.Length - 2);
                json += "] }";

                //json = "{ \"recognized\": [\"FIRST PAWN TWO\"] }";
                Console.WriteLine(json);

                var exNot = lce.ExtensionNotification(e.Result.Audio.StartTime + "", e.Result.Audio.StartTime.Add(e.Result.Audio.Duration) + "", e.Result.Confidence, json);
                mmic.Send(exNot);
            }
            else {
                tts.Speak("O seu pedido não foi muito claro. Importa-se de repetir ?");
            }
        }


        //  NEW 16 April 2020   - create receiver, answer to messages received
        //  Adapted from AppGUI code






        //MmiReceived_Message;

        private void MmiReceived_Message(object sender, MmiEventArgs e)
        {
            //Console.WriteLine(e.Message);

            var doc = XDocument.Parse(e.Message);
            var com = doc.Descendants("command").FirstOrDefault().Value;

            Console.WriteLine(doc);


            //tts.Speak(com);

            //dynamic json = JsonConvert.DeserializeObject(com);

            //switch ((string)json.recognized[0].ToString())
            switch (com)
            {
                case "giveup":
                    //tts.Speak("O jogo foi abortado. Deseja começar um novo?");
                    new_game_flag = true;
                    break;

                case "newgame":
                    if (new_game_flag) { 
                        tts.Speak("Pode começar. Boa sorte!");
                        new_game_flag = false;
                    }
                    break;
                case "record":
                    tts.Speak("Esta seria uma boa jogada. Deseja fazê-la ?");
                    hint_flag = true;
                    break;
            }

            
        }
    }
}
