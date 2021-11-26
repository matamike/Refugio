using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

public class RefugeePropertiesSelectionPool : MonoBehaviour
{


    //Properties pool holders
    int[] refugeeGender = { 0, 1 };
    List<string> maleRefugeesNames = new List<string>();
    List<string> femaleRefugeesNames = new List<string>();
    List<string> refugeeSurnames = new List<string>();
    List<string> refugeePlacesOfBirth = new List<string>();
    List<string> familyRefugeeSurnames = new List<string>();
    List<string> maleRefugeeProfessions = new List<string>();
    List<string> femaleRefugeeProfessions = new List<string>();

    //Add Icons Images TODO

    //Conditions property holder
    //List<string> healthConditions;
    //List<string> hungerConditions;
    //List<string> emotionalConditions;
    //List<string> otherConditions;
    //List<string> femaleConditions;

    //////////////////////////////////////////

    //Frequencies of Values over application lifetime. (MAYBE REMOVE)

    //List<(string, int)> maleRefugeeNamesFreqs = new List<(string, int)>();
    //List<(string, int)> femaleRefugeeNamesFreqs = new List<(string, int)>();
    //List<(string, int)> refugeeSurnamesFreqs = new List<(string, int)>();
    //List<(string, int)> refugeeFamilySurnamesFreqs = new List<(string, int)>();
    //List<(string, int)> placesOfBirthFreqs = new List<(string, int)>();



    void Awake()
    {
        ReadAttributes();
    }

    //Public Methods

    public List<string> GetRefugeesSurnames()
    {
        return refugeeSurnames;
    }

    public List<string> GetRefugeeFamilySurnames()
    {
        return familyRefugeeSurnames;
    }

    public List<string> GetMaleRefugeeProfessions()
    {
        return maleRefugeeProfessions;
    }

    public List<string> GetFemaleRefugeeProfessions()
    {
        return femaleRefugeeProfessions;
    }

    public List<string> GetRefugeeBirthplaces()
    {
        return refugeePlacesOfBirth;
    }


    //Private Methods

    //Get All Attributes from File 
    async void ReadAttributes()
    {
        bool isRead = false;

        ReadSurnames();
        ReadFamilySurnames();
        ReadFemaleNames();
        ReadMaleNames();
        ReadFemaleProfessions();
        ReadMaleProfessions();
        ReadPlacesOfBirth();

        while (!isRead) await Task.Yield();
    }

    //Get Surnames from File
    void ReadSurnames()
    {
        TextAsset txtAsset = (TextAsset)Resources.Load("surnames");
        string surnames = txtAsset.text;
        string[] stringArr = surnames.Split('\n');
        foreach (string surname in stringArr)
        {
            refugeeSurnames.Add(surname);
        }
    }

    //Get Family Surnames from File
    void ReadFamilySurnames()
    {
        TextAsset txtAsset = (TextAsset)Resources.Load("familySurnames");
        string familyRefSurnames = txtAsset.text;
        string[] stringArr = familyRefSurnames.Split('\n');
        foreach (string familyRefugeeSurname in stringArr)
        {
            familyRefugeeSurnames.Add(familyRefugeeSurname);
        }
    }

    //Get Female Names from File
    void ReadFemaleNames()
    {
        TextAsset txtAsset = (TextAsset)Resources.Load("femaleNames");
        string femaleNames = txtAsset.text;
        string[] stringArr = femaleNames.Split('\n');
        foreach (string femaleName in stringArr)
        {
            femaleRefugeesNames.Add(femaleName);
        }
    }

    //Get Male Names from File
    void ReadMaleNames()
    {
        TextAsset txtAsset = (TextAsset)Resources.Load("maleNames");
        string maleNames = txtAsset.text;
        string[] stringArr = maleNames.Split('\n');
        foreach (string maleName in stringArr)
        {
            maleRefugeesNames.Add(maleName);
        }
    }

    //Get Places of Birth from File
    void ReadPlacesOfBirth()
    {
        TextAsset txtAsset = (TextAsset)Resources.Load("placesOfBirth");
        string placesOfBirth = txtAsset.text;
        string[] stringArr = placesOfBirth.Split('\n');
        foreach (string placeOfBirth in stringArr)
        {
            refugeePlacesOfBirth.Add(placeOfBirth);
        }
    }

    //Get Male Profession from File
    void ReadMaleProfessions()
    {
        TextAsset txtAsset = (TextAsset)Resources.Load("professions");
        string maleProfessions = txtAsset.text;
        string[] stringArr = maleProfessions.Split('\n');
        foreach (string maleProfession in stringArr)
        {
            maleRefugeeProfessions.Add(maleProfession);
        }
    }

    //Get Female Professions from File
    void ReadFemaleProfessions()
    {
        TextAsset txtAsset = (TextAsset)Resources.Load("professions");
        string femaleProfessions = txtAsset.text;
        string[] stringArr = femaleProfessions.Split('\n');
        foreach (string femaleProfession in stringArr)
        {
            femaleRefugeeProfessions.Add(femaleProfession);
        }
    }

    ///////////////////////////////////

    //Public Methods

    //Generate Random Attributes for Individuals
    public List<(string, string)> GenerateProfile(int gender)
    {
        List<(string, string)> properties = new List<(string, string)>();

        //Male
        if (gender == 0)
        {
            string refugeeName = maleRefugeesNames[Random.Range(0, maleRefugeesNames.Count)];
            string refugeeSurname = refugeeSurnames[Random.Range(0, refugeeSurnames.Count)];
            string refugeeBirthPlace = refugeePlacesOfBirth[Random.Range(0, refugeePlacesOfBirth.Count)];
            string refugeeProfession = maleRefugeeProfessions[Random.Range(0, maleRefugeeProfessions.Count)];
            properties.Add(("Name", refugeeName));
            properties.Add(("Surname", refugeeSurname));
            properties.Add(("Birthplace", refugeeBirthPlace));
            properties.Add(("Profession", refugeeProfession));

            return properties;
        }

        //Female
        if (gender == 1)
        {
            string refugeeName = femaleRefugeesNames[Random.Range(0, femaleRefugeesNames.Count)];
            string refugeeSurname = refugeeSurnames[Random.Range(0, refugeeSurnames.Count)];
            string refugeeBirthPlace = refugeePlacesOfBirth[Random.Range(0, refugeePlacesOfBirth.Count)];
            string refugeeProfession = maleRefugeeProfessions[Random.Range(0, maleRefugeeProfessions.Count)]; //femaleRefugeeProfessions[Random.Range(0, femaleRefugeeProfessions.Count)]; (empty)
            properties.Add(("Name", refugeeName));
            properties.Add(("Surname", refugeeSurname));
            properties.Add(("Birthplace", refugeeBirthPlace));
            properties.Add(("Profession", refugeeProfession));

            return properties;
        }

        //Unknown Gender case
        return properties;
    }


    //Generate Random Attributes for Family Members
    public List<(string, string)> GenerateFamilyProfile(int gender, bool isChild = false, bool isParent = false)
    {
        List<(string, string)> properties = new List<(string, string)>();
        //Family Surname is the same for each Member.
        string familySurname = familyRefugeeSurnames[Random.Range(0, familyRefugeeSurnames.Count)];

        //Parent
        if (isParent)
        {
            if (gender == 0)
            {
                string refugeeName = maleRefugeesNames[Random.Range(0, maleRefugeesNames.Count)];
                string refugeeBirthPlace = refugeePlacesOfBirth[Random.Range(0, refugeePlacesOfBirth.Count)];
                string refugeeProfession = maleRefugeeProfessions[Random.Range(0, maleRefugeeProfessions.Count)];
                properties.Add(("Name", refugeeName));
                properties.Add(("Surname", familySurname));
                properties.Add(("Birthplace", refugeeBirthPlace));
                properties.Add(("Profession", refugeeProfession));

                return properties;
            }

            if (gender == 1)
            {
                string refugeeName = femaleRefugeesNames[Random.Range(0, femaleRefugeesNames.Count)];
                string refugeeBirthPlace = refugeePlacesOfBirth[Random.Range(0, refugeePlacesOfBirth.Count)];
                string refugeeProfession = maleRefugeeProfessions[Random.Range(0, maleRefugeeProfessions.Count)];//femaleRefugeeProfessions[Random.Range(0, femaleRefugeeProfessions.Count)];
                properties.Add(("Name", refugeeName));
                properties.Add(("Surname", familySurname));
                properties.Add(("Birthplace", refugeeBirthPlace));
                properties.Add(("Profession", refugeeProfession));

                return properties;
            }

            //unkown gender
            return null;
        }
        //Child 
        if (isChild)
        {
            if (gender == 0)
            {
                string refugeeName = maleRefugeesNames[Random.Range(0, maleRefugeesNames.Count)];
                string refugeeBirthPlace = refugeePlacesOfBirth[Random.Range(0, refugeePlacesOfBirth.Count)];
                properties.Add(("Name", refugeeName));
                properties.Add(("Surname", familySurname));
                properties.Add(("Birthplace", refugeeBirthPlace));
                properties.Add(("Profession", "Unemployed"));

                return properties;
            }
            if (gender == 1)
            {
                string refugeeName = femaleRefugeesNames[Random.Range(0, femaleRefugeesNames.Count)];
                string refugeeBirthPlace = refugeePlacesOfBirth[Random.Range(0, refugeePlacesOfBirth.Count)];
                properties.Add(("Name", refugeeName));
                properties.Add(("Surname", familySurname));
                properties.Add(("Birthplace", refugeeBirthPlace));
                properties.Add(("Profession", "Unemployed"));

                return properties;
            }

            //unkown gender
            return null;
        }

        //unknown family role.
        return null;
    }

}
