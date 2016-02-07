using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Shot {

	static public List<Shot> shots = new List<Shot> ();
	static public string prefsName = "QuickSnap_Shots";

	public Vector3 position;
	public Quaternion rotation;
	public Vector3 target;

	public string ToXML()
	{
		string ss = "<shot ";
		ss += "x=\""+position.x+"\" ";
		ss += "y=\""+position.y+"\" ";
		ss += "z=\""+position.z+"\" ";
		ss += "qx=\"" + rotation.x + "\" ";
		ss += "qy=\"" + rotation.y + "\" ";
		ss += "qz=\"" + rotation.z + "\" ";
		ss += "qw=\"" + rotation.w + "\" ";
		ss += "tx=\""+target.x+"\" ";
		ss += "ty=\""+target.y+"\" ";
		ss += "tz=\""+target.z+"\" ";
		ss += " />";

		return(ss);
	}

	static public Shot ParseShotXML(PT_XMLHashtable xHT)
	{
		Shot sh = new Shot ();

		sh.position.x = float.Parse(xHT.att("x"));
		sh.position.y = float.Parse(xHT.att("y"));
		sh.position.z = float.Parse(xHT.att("z"));
		sh.rotation.x = float.Parse(xHT.att("qx"));
		sh.rotation.y = float.Parse(xHT.att("qy"));
		sh.rotation.z = float.Parse(xHT.att("qz"));
		sh.rotation.w = float.Parse(xHT.att("qw"));
		sh.target.x = float.Parse(xHT.att("tx"));
		sh.target.y = float.Parse(xHT.att("ty"));
		sh.target.z = float.Parse(xHT.att("tz"));

		return (sh);
	}

	static public void LoadShots()
	{
		shots = new List<Shot> ();

		if (!PlayerPrefs.HasKey(prefsName))
		{
			return;
		}

		string shotsXML = PlayerPrefs.GetString (prefsName);
		PT_XMLReader xmlr = new PT_XMLReader ();
		xmlr.Parse (shotsXML);

		PT_XMLHashList hl = xmlr.xml["xml"] [0] ["shot"];
		for (int i=0; i<hl.Count; i++)
		{
			PT_XMLHashtable ht = hl[i];
			Shot sh = ParseShotXML(ht);shots.Add(sh);
		}
	}

	static public void SaveShots()
	{
		string xs = Shot.XML;

		Utils.tr (xs);

		PlayerPrefs.SetString (prefsName, xs);

		Utils.tr("PlayerPrefs. "+prefsName+" has been set.");
	}

	static public string XML 
	{
		get
		{
			string xs = "<xml\n>";
			foreach (Shot sh in shots)
			{
				xs += sh.ToXML()+"\n";
			}
			xs += "</xml>";
			return(xs);
		}
	}

	static public void DeleteShots()
	{
		shots = new List<Shot> ();
		if (PlayerPrefs.HasKey(prefsName))
		{
			PlayerPrefs.DeleteKey(prefsName);
			Utils.tr("PlayerPrefs."+prefsName+" has been deleted.");
		} else {
			Utils.tr("There was no PlayerPrefs."+prefsName+" to delete.");
		}
	}

	static public void ReplaceShot(int ndx, Shot sh)
	{
		if (shots==null || shots.Count <= ndx) return;
		shots.RemoveAt(ndx);
		shots.Insert(ndx, sh);

		Utils.tr("Replaced shot:", ndx, "with", sh.ToXML());
	}

	public static float Compare(Shot target, Shot test)
	{
		float posDev = (test.position - target.position).magnitude;
		float tarDev = (test.target - target.target).magnitude;
////////////
		float posAccPct, tarAccPct, posAP2, tarAP2;
		TargetCamera tc = TargetCamera.S;

		posAccPct = 1 - (posDev / tc.maxPosDeviation);
		tarAccPct = 1 - (tarDev / tc.maxTarDeviation);

		posAP2 = Easing.Ease (posAccPct, tc.deviationEasing);
		tarAP2 = Easing.Ease (tarAccPct, tc.deviationEasing);

		float accuracy = (posAP2 + tarAP2) / 2f;

		string accText = Utils.RoundToPlaces(accuracy*100).ToString()+"%";
		Utils.tr ("Position:", posAccPct, posAP2, "Target:", tarAccPct, tarAP2, "Accuracy", accText);

		return(accuracy);
	}

}
