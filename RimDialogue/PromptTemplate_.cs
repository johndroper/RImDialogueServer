﻿using RimDialogue.Core;
using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RimDialogue
{
  public class FloatData
  {
    public FloatData(string label, float value)
    {
      Label = label;
      Value = value;
    }
    public string Label { get; set; }
    public float Value { get; set; }
  }


  public partial class PromptTemplate
  {
    public static Regex GetDataRegex = new(@"(\w+)\s\[([+-]?[0-9]+[.]?[0-9]+)\]", RegexOptions.Compiled);

    public static FloatData? GetFloatData(string data)
    {
      Match match = GetDataRegex.Match(data);
      if (match.Success)
      {
        string label = match.Groups[1].Value;
        float value = float.Parse(match.Groups[2].Value);
        return new FloatData(label, value);
      }
      return null;
    }

    public static string TemperatureFeel(float temperatureCelsius)
    {
      if (temperatureCelsius <= -30.0f)
        return "freezing";
      else if (temperatureCelsius <= -10.0f)
        return "bitter cold";
      else if (temperatureCelsius <= 0.0f)
        return "very cold";
      else if (temperatureCelsius <= 10.0f)
        return "chilly";
      else if (temperatureCelsius <= 15.0f)
        return "cool";
      else if (temperatureCelsius <= 20.0f)
        return "mild";
      else if (temperatureCelsius <= 25.0f)
        return "comfortable";
      else if (temperatureCelsius <= 30.0f)
        return "warm";
      else if (temperatureCelsius <= 35.0f)
        return "hot";
      else if (temperatureCelsius <= 40.0f)
        return "very hot";
      else
        return "scorching";
    }

    public static string DaysAgo(int numberOfDaysAgo)
    {
      if (numberOfDaysAgo < 0)
        throw new ArgumentOutOfRangeException(nameof(numberOfDaysAgo), "The number of days cannot be negative.");

      if (numberOfDaysAgo == 0)
        return "today";
      else if (numberOfDaysAgo == 1)
        return "yesterday";
      else if (numberOfDaysAgo <= 2)
        return "a couple of days ago";
      else if (numberOfDaysAgo <= 3)
        return "a few days ago";
      else if (numberOfDaysAgo <= 4)
        return "about a week ago";
      else if (numberOfDaysAgo <= 7)
        return "a couple of weeks ago";
      else if (numberOfDaysAgo <= 15)
        return "about a month ago";
      else if (numberOfDaysAgo <= 30)
        return "a month or two ago";
      else if (numberOfDaysAgo <= 45)
        return "a few months ago";
      else if (numberOfDaysAgo <= 60)
        return "about a year ago";
      else if (numberOfDaysAgo <= 120)
        return "over a year ago";
      else if (numberOfDaysAgo <= 180)
        return "a couple of years ago";
      else if (numberOfDaysAgo <= 300)
        return "several years back";
      else
        return "a very long time ago";
    }

    public static string FeelingsLabel(float? value)
    {
      if (value == null)
        return "None";

      if (value < -100.0f || value > 100.0f)
        throw new ArgumentOutOfRangeException(nameof(value), "Value must be between -100.0 and 100.0.");

      if (value <= -20.0f)
        return "devastated by";
      else if (value <= -15.0f)
        return "hurt by";
      else if (value <= -10.0f)
        return "upset by";
      else if (value <= -5.0f)
        return "disappointed with";
      else if (value <= -1.0f)
        return "let down by";
      else if (value < 1.0f)
        return "indifferent to";
      else if (value <= 5.0f)
        return "content with";
      else if (value <= 10.0f)
        return "pleased by";
      else if (value <= 15.0f)
        return "happy with";
      else if (value <= 20.0f)
        return "delighted by";
      else
        return "thrilled by";
    }

    public static string WealthDescription(float wealth)
    {
      if (wealth < 0)
        throw new ArgumentOutOfRangeException(nameof(wealth), "Wealth must be greater than 0.");

      if (wealth == 0)
        return "The colony is destitute, barely scraping by with no resources.";
      else if (wealth <= 5_000)
        return "The colony is very poor, with only the most basic resources to survive.";
      else if (wealth <= 20_000)
        return "The colony is poor, struggling to meet basic needs.";
      else if (wealth <= 50_000)
        return "The colony has modest resources, enough to sustain a simple lifestyle.";
      else if (wealth <= 100_000)
        return "The colony is getting by, with enough resources to be comfortable, but nothing luxurious.";
      else if (wealth <= 200_000)
        return "The colony is comfortable, with decent resources and a sense of security.";
      else if (wealth <= 400_000)
        return "The colony is well-off, with plentiful resources and ample supplies.";
      else if (wealth <= 600_000)
        return "The colony is affluent, with a wealth of resources and an enviable lifestyle.";
      else if (wealth <= 800_000)
        return "The colony is wealthy, with substantial assets and high standards of living.";
      else if (wealth <= 900_000)
        return "The colony is very wealthy, with extensive resources and luxurious amenities.";
      else
        return "The colony is a powerhouse, reaching millionaire status with a vast wealth of resources.";
    }

    public PromptTemplate(
      DialogueData dialogueData, 
      List<Conversation>? initiatorConversations, 
      List<Conversation>? recipientConversations)
    {
      DialogueData = dialogueData;
      InitiatorConversations = initiatorConversations;
      RecipientConversations = recipientConversations;
    }

    public bool ShowFullName { get; } = false;
    public bool ShowGender { get; } = false;
    public bool ShowRace { get; } = false;
    public bool ShowSubrace { get; } = false;
    public bool ShowAnimal { get; } = false;
    public bool ShowAge { get; } = false;
    public bool ShowIsColonist { get; } = false;
    public bool ShowIsPrisoner { get; } = false;
    public bool ShowRoyaltyTitle { get; } = false;
    public bool ShowHair { get; } = false;
    public bool ShowBeard { get; } = false;
    public bool ShowTattoo { get; } = false;
    public bool ShowFaction { get; } = false;
    public bool ShowIdeology { get; } = false;
    public bool ShowPrecepts { get; } = false;
    public bool ShowAdulthood { get; } = false;
    public bool ShowChildhood { get; } = false;
    public bool ShowRelations { get; } = false;
    public bool ShowTraits { get; } = false;
    public bool ShowSkills { get; } = false;
    public bool ShowMoodThoughts { get; } = false;
    public bool ShowHealth { get; } = false;
    public bool ShowApparel { get; } = false;
    public bool ShowWeapons { get; } = false;
    public bool ShowMoodString { get; } = false;
    public bool ShowNeeds { get; } = false;
    public bool ShowOpinions { get; } = false;
    public bool ShowConversations { get; } = false;
    public bool ShowScenario { get; } = true;
    public bool ShowDaysAgo { get; } = false;
    public bool ShowWealth { get; } = false;
    public bool ShowBiome { get; } = false;
    public bool ShowRoom { get; } = false;
    public bool ShowWeather { get; } = false;
    public bool ShowRecentIncidents { get; } = false;
    public bool RepeatInstructions { get; } = false;
    public bool ShowSpecialInstructions { get; } = false;
    public bool ShowExposition { get; } = true;
    public DialogueData DialogueData { get; }
    public List<Conversation>? InitiatorConversations { get; set; }
    public List<Conversation>? RecipientConversations { get; set; }

    public string GetThoughts(string name, string[] thoughts)
    {
      return string.Join(", ", this.DialogueData.initiatorOpinionOfRecipient.Select(opinion => GetThoughtLabel(name, opinion)));
    }

    public string GetThoughtLabel(string name, string data)
    {
      FloatData? floatData = GetFloatData(data);
      if (floatData != null)
        return $"{floatData.Label} - {name} was {FeelingsLabel(floatData.Value)} this";
      return data;
    }

    public string PercentToLabel(float value)
    {
      if (value == 0)
        return "none";
      else if (value <= 0.1)
        return "extremely low";
      else if (value <= 0.2)
        return "very low";
      else if (value <= 0.3)
        return "low";
      else if (value <= 0.4)
        return "below average";
      else if (value <= 0.5)
        return "moderate";
      else if (value <= 0.6)
        return "normal";
      else if (value <= 0.7)
        return "above average";
      else if (value <= 0.8)
        return "high";
      else if (value <= 0.9)
        return "very high";
      else if (value < 1)
        return "extremely high";
      else if (value == 1)
        return "maximum";
      else
        return "unknown";
    }

    public string DaysAgoLabel
    {
      get
      {
        return DaysAgo(DialogueData.daysPassedSinceSettle);
      }
    }
  }
}
