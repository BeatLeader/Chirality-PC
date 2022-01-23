﻿using System.Collections.Generic;

namespace Chirality
{
    class MirrorTransforms
    {
        internal static System.Random rand;
        internal static List<NoteCutDirection> directions = new List<NoteCutDirection> {NoteCutDirection.Up, NoteCutDirection.Down, NoteCutDirection.Left , NoteCutDirection.Right,
                                                                                        NoteCutDirection.UpLeft, NoteCutDirection.UpRight, NoteCutDirection.DownLeft, NoteCutDirection.DownRight,
                                                                                        NoteCutDirection.Any, NoteCutDirection.None};
        internal static Dictionary<NoteCutDirection, NoteCutDirection> horizontal_cut_transform;
        internal static Dictionary<NoteCutDirection, NoteCutDirection> vertical_cut_transform;


        internal static BeatmapData Mirror_Horizontal(BeatmapData beatmapData, bool flip_lines, bool remove_walls, bool is_ME)
        {
			//Plugin.Log.Debug("Mirror Horizontal");

			int numberOfLines = beatmapData.numberOfLines;
			BeatmapData h_beatmapData = new BeatmapData(numberOfLines);

			foreach (BeatmapObjectData beatmapObjectData in beatmapData.beatmapObjectsData)
			{
                NoteData noteData;
                if ((noteData = (beatmapObjectData as NoteData)) != null)
                {
                    h_beatmapData.AddBeatmapObjectData(Mirror_Horizontal_Note(noteData, numberOfLines, flip_lines, is_ME));
                }

                if (remove_walls == false)
                {
                    ObstacleData obstacleData;
                    if ((obstacleData = (beatmapObjectData as ObstacleData)) != null)
                    {
                        h_beatmapData.AddBeatmapObjectData(Mirror_Horizontal_Obstacle(obstacleData, numberOfLines, flip_lines));
                    }
                }
            }

            foreach (BeatmapEventData beatmapEventData in beatmapData.beatmapEventsData)
            {
                h_beatmapData.AddBeatmapEventData(beatmapEventData);
            }

            foreach (KeyValuePair<string, HashSet<BeatmapEventType>> keyValuePair in beatmapData.availableSpecialEventsPerKeywordDictionary)
            {
                h_beatmapData.AddAvailableSpecialEventsPerKeyword(keyValuePair.Key, keyValuePair.Value);
            }

			return h_beatmapData;
		}


        internal static BeatmapData Mirror_Vertical(BeatmapData beatmapData, bool flip_rows, bool remove_walls, bool is_ME)
        {
            //Plugin.Log.Debug("Mirror Vertical");

            int numberOfLines = beatmapData.numberOfLines;
            BeatmapData v_beatmapData = new BeatmapData(numberOfLines);

            foreach (BeatmapObjectData beatmapObjectData in beatmapData.beatmapObjectsData)
            {
                NoteData noteData;
                if ((noteData = (beatmapObjectData as NoteData)) != null)
                {
                    v_beatmapData.AddBeatmapObjectData(Mirror_Vertical_Note(noteData, flip_rows, is_ME));
                }

                if (remove_walls == false)
                {
                    ObstacleData obstacleData;
                    if ((obstacleData = (beatmapObjectData as ObstacleData)) != null)
                    {
                        v_beatmapData.AddBeatmapObjectData(Mirror_Vertical_Obstacle(obstacleData, flip_rows));
                    }
                }
            }

            foreach (BeatmapEventData beatmapEventData in beatmapData.beatmapEventsData)
            {
                v_beatmapData.AddBeatmapEventData(beatmapEventData);
            }

            foreach (KeyValuePair<string, HashSet<BeatmapEventType>> keyValuePair in beatmapData.availableSpecialEventsPerKeywordDictionary)
            {
                v_beatmapData.AddAvailableSpecialEventsPerKeyword(keyValuePair.Key, keyValuePair.Value);
            }

            return v_beatmapData;
        }


        internal static BeatmapData Mirror_Inverse(BeatmapData beatmapData, bool flip_lines, bool flip_rows, bool remove_walls, bool is_ME)
        {
            //Plugin.Log.Debug("Mirror Inverse");

            return Mirror_Vertical(Mirror_Horizontal(beatmapData, flip_lines, remove_walls, is_ME), flip_rows, remove_walls, is_ME);
        }

        internal static NoteCutDirection Get_Random_Direction()
        {
            int index = rand.Next(directions.Count);

            return directions[index];
        }


        #region "Horizontal Transform Functions"

        internal static void Create_Horizontal_Transforms()
        {
            Plugin.Log.Debug("Create Horizontal Transforms");

            horizontal_cut_transform = new Dictionary<NoteCutDirection, NoteCutDirection>();

            horizontal_cut_transform.Add(NoteCutDirection.Up, NoteCutDirection.Up);
            horizontal_cut_transform.Add(NoteCutDirection.Down, NoteCutDirection.Down);

            horizontal_cut_transform.Add(NoteCutDirection.UpLeft, NoteCutDirection.UpRight);
            horizontal_cut_transform.Add(NoteCutDirection.DownLeft, NoteCutDirection.DownRight);

            horizontal_cut_transform.Add(NoteCutDirection.UpRight, NoteCutDirection.UpLeft);
            horizontal_cut_transform.Add(NoteCutDirection.DownRight, NoteCutDirection.DownLeft);

            horizontal_cut_transform.Add(NoteCutDirection.Left, NoteCutDirection.Right);
            horizontal_cut_transform.Add(NoteCutDirection.Right, NoteCutDirection.Left);
            horizontal_cut_transform.Add(NoteCutDirection.Any, NoteCutDirection.Any);
            horizontal_cut_transform.Add(NoteCutDirection.None, NoteCutDirection.None);
        }


        private static NoteData Mirror_Horizontal_Note(NoteData noteData, int numberOfLines, bool flip_lines, bool is_ME)
        {
            int h_lineIndex;

            // Extended maps will not have lines flipped (lazy to do the math) but their colors will
            // Yes, it will be weird, hence chaos mode
            // Also Note: Not worth reusing check function because non-extend block will become unnecessarily complicated

            if (noteData.lineIndex >= 1000 || noteData.lineIndex <= -1000)
            {
                h_lineIndex = noteData.lineIndex / 1000 - 1; // Definition from ME
            }

            // Keep This Note: This isn't a robust way to check for extended maps
            /*if (noteData.lineIndex > 10 || noteData.lineIndex < 0) 
            {
                //h_lineIndex = noteData.lineIndex / 1000;
                h_lineIndex = rand.Next(4); // ME chaos mode kekeke turns out this is too chaotic, not that fun
            }*/

            // Option to only flip non-extended maps 
            else if (flip_lines)
            {
                h_lineIndex = numberOfLines - 1 - noteData.lineIndex;
            }
            else
            {
                h_lineIndex = noteData.lineIndex;
            }

            NoteCutDirection h_cutDirection; // Yes, this is support for precision placement and ME LOL
            if (horizontal_cut_transform.TryGetValue(noteData.cutDirection, out h_cutDirection) == false || is_ME)
            {
                h_cutDirection = Get_Random_Direction();
            }

            NoteData h_noteData = NoteData.CreateBasicNoteData(noteData.time, h_lineIndex, Check_Layer(noteData.noteLineLayer), noteData.colorType.Opposite(), h_cutDirection);

            return h_noteData;
        }


        private static ObstacleData Mirror_Horizontal_Obstacle(ObstacleData obstacleData, int numberOfLines, bool flip_lines)
        {
            ObstacleData h_obstacleData;
            if (flip_lines && obstacleData.obstacleType == ObstacleType.FullHeight)
            {
                h_obstacleData = new ObstacleData(obstacleData.time, numberOfLines - obstacleData.width - obstacleData.lineIndex, ObstacleType.FullHeight, obstacleData.duration, obstacleData.width);
                return h_obstacleData;
            }

            return obstacleData;
        }
        #endregion


        #region "Vertical Transform Functions"

        internal static void Create_Vertical_Transforms()
        {
            Plugin.Log.Debug("Create Vertical Transforms");

            vertical_cut_transform = new Dictionary<NoteCutDirection, NoteCutDirection>();

            vertical_cut_transform.Add(NoteCutDirection.Up, NoteCutDirection.Down);
            vertical_cut_transform.Add(NoteCutDirection.Down, NoteCutDirection.Up);

            vertical_cut_transform.Add(NoteCutDirection.UpLeft, NoteCutDirection.DownLeft);
            vertical_cut_transform.Add(NoteCutDirection.DownLeft, NoteCutDirection.UpLeft);

            vertical_cut_transform.Add(NoteCutDirection.UpRight, NoteCutDirection.DownRight);
            vertical_cut_transform.Add(NoteCutDirection.DownRight, NoteCutDirection.UpRight);

            vertical_cut_transform.Add(NoteCutDirection.Left, NoteCutDirection.Left);
            vertical_cut_transform.Add(NoteCutDirection.Right, NoteCutDirection.Right);
            vertical_cut_transform.Add(NoteCutDirection.Any, NoteCutDirection.Any);
            vertical_cut_transform.Add(NoteCutDirection.None, NoteCutDirection.None);
        }


        private static NoteData Mirror_Vertical_Note(NoteData noteData, bool flip_rows, bool has_ME)
        {
            NoteLineLayer v_noteLinelayer;

            if ((int)noteData.noteLineLayer >= 1000 || (int)noteData.noteLineLayer <= -1000)
            {
                v_noteLinelayer = (NoteLineLayer)((int)noteData.noteLineLayer / 1000) - 1;
            }
            /*if ((int)noteData.noteLineLayer > 2) 
            {
                v_noteLinelayer = (NoteLineLayer)rand.Next(3); // ME chaos mode
            }*/
            else if (flip_rows)
            {
                v_noteLinelayer = (NoteLineLayer)(3 - 1 - (int)noteData.noteLineLayer);
            }
            else
            {
                v_noteLinelayer = noteData.noteLineLayer;
            }

            NoteCutDirection v_cutDirection;
            if (vertical_cut_transform.TryGetValue(noteData.cutDirection, out v_cutDirection) == false || has_ME)
            {
                v_cutDirection = Get_Random_Direction();
            }

            NoteData v_noteData = NoteData.CreateBasicNoteData(noteData.time, Check_Index(noteData.lineIndex), v_noteLinelayer, noteData.colorType, v_cutDirection);

            return v_noteData;
        }


        private static ObstacleData Mirror_Vertical_Obstacle(ObstacleData obstacleData, bool flip_rows)
        {
            if (flip_rows && obstacleData.obstacleType == ObstacleType.Top)
            {
                obstacleData.MoveTime(-1); // To keep the number of walls the same
            }

            return obstacleData;
        }
        #endregion


        #region "Check Functions"
        internal static int Check_Index(int lineIndex)
        {
            if (lineIndex >= 500 || lineIndex <= -500)
            {
                return lineIndex / 1000;
                //return rand.Next(4);
            }

            return lineIndex;
        }

        internal static NoteLineLayer Check_Layer(NoteLineLayer lineLayer)
        {
            if ((int)lineLayer >= 500 || (int)lineLayer <= -500)
            {
                return (NoteLineLayer)((int)lineLayer / 1000);
                //return (NoteLineLayer)rand.Next(3); // ME chaos mode
            }

            return lineLayer;
        }
        #endregion
    }
}
