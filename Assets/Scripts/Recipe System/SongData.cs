using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;


[CreateAssetMenu(menuName = "RecipeSystemScriptableObjects/New Song")]
public class SongData : ScriptableObject
{
    [SerializeField] string songName;
    [SerializeField] float duration;
    [SerializeField] RecipeData[] recipeDataList;
    [SerializeField] float songBPM;
    public AudioClip drumTrack;
    recipe[] componentRecipes;

    public song Song
    {
        get
        {
            GenerateRecipes();
            return new song(songName, duration, componentRecipes, drumTrack, songBPM);
        }
    }

    public void GenerateRecipes()
    {
        recipe[] finalList = new recipe[recipeDataList.Length];
        for (int i = 0; i < recipeDataList.Length; i++)
        {
            finalList[i] = recipeDataList[i].Recipe;
        }
        componentRecipes = finalList;
    }
}

/*[CustomEditor(typeof(SongDataEditor))]
public class SongDataEditor : Editor
{
    private SongData songDataScript;

    private void OnEnable()
    {
        if (target is SongData) songDataScript = target as SongData;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("GenerateRecipeList"))
        {
            songDataScript.GenerateRecipes();
        }
    }
}*/
