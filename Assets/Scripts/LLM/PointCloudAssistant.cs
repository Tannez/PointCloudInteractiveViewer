using UnityEngine;
using LLMUnity;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
public static class PCFunctions
{
    static System.Random random = new System.Random();
    public static string PointBudget()
    {
        string[] pointBudget = new string[]{"200.000", "400.000", "750.000", "1.000.000"};
        return "The Point Budget of the Point Cloud is " + pointBudget[random.Next(pointBudget.Length)];
    }
    public static string PCClassCount()
    {
        return "The Point Cloud contains: " + random.Next(5).ToString("D2") + " Classes, with: " + random.Next(10).ToString("D2") + " Total Instances";
    }
    public static string PCColorMode()
    {
        string[] pCColorsMode = new string[]{"RGBA", "Classification", "Intensity"};
        return "Current color mode is set to: " + pCColorsMode[random.Next(pCColorsMode.Length)];
    }
}
public class PointCloudAssistant : MonoBehaviour
{
    public LLMCharacter llmCharacter;
    public InputField playerText;
    public Text AIText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerText.onSubmit.AddListener(onInputFieldSubmit);
        playerText.Select();
        llmCharacter.grammarString = MultipleChoiceGrammar();
    }

    // Get the available functions within the Functions class
    string[] GetFunctionNames()
    {
        List<string> functionNames = new List<string>();
        foreach (var function in typeof(PCFunctions).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)) functionNames.Add(function.Name);
        return functionNames.ToArray();
    }

    // Specfify grammar setup for the LLM (This one is set up for multiple choice responses)
    string MultipleChoiceGrammar()
    {
        return "root ::= (\"" + string.Join("\" | \"", GetFunctionNames()) + "\")";
    }

    // The prompt the LLM returns to the user
    string ConstructPrompt(string message)
    {
        string prompt = "Which of the following choices matches best the input?\n\n";
        prompt += "Input:" + message + "\n\n";
        prompt += "Choices:\n";
        foreach (string functionName in GetFunctionNames()) prompt += $"- {functionName}\n";
        prompt += "\nAnswer directly with the choice";
        return prompt;
    }

    // Function within the function class which the LLM will try and call
    string CallFunction(string functionName)
    {
        return (string)typeof(PCFunctions).GetMethod(functionName).Invoke(null, null);
    }

    // Method for when prompt has been submitted
    async void onInputFieldSubmit(string message)
    {
        playerText.interactable = false;
        string functionName = await llmCharacter.Chat(ConstructPrompt(message));
        string result = CallFunction(functionName);
        AIText.text = $"Calling {functionName}\n{result}";
        playerText.interactable = true;
    }
    
    public void CancelRequests()
    {
        llmCharacter.CancelRequests();
    }

    public void ExitGame()
    {
        Debug.Log("Exit button clicked");
        Application.Quit();
    }

    // Ensure that an LLM Model is selected
    bool onValidateWarning = true;
    void OnValidate()
    {
        if (onValidateWarning && !llmCharacter.remote && llmCharacter.llm != null && llmCharacter.llm.model == "")
        {
            Debug.LogWarning($"Please select a model in the {llmCharacter.llm.gameObject.name} GameObject!");
            onValidateWarning = false;
        }
    }
}
