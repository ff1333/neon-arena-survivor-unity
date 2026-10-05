using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public sealed class PortfolioSettingsMenu : MonoBehaviour
{
    private GameObject panel;
    private float previousTimeScale;
    private AudioSource music;
    private AudioClip musicClip;
    private Button chineseButton, englishButton;
    private Toggle sound;
    public static PortfolioSettingsMenu Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register()
    {
        SceneManager.sceneLoaded -= Loaded;
        SceneManager.sceneLoaded += Loaded;
    }
    private static void Loaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main") new GameObject("Settings Service").AddComponent<PortfolioSettingsMenu>();
    }
    private IEnumerator Start()
    {
        Instance = this;
        yield return null;
        PortfolioSettings.Apply();
        PortfolioSettings.Changed += Refresh;
        var arenaMenu = GameObject.Find("StartPanel");
        var mazeMenu = GameObject.Find("Title Screen");
        var menu = arenaMenu != null ? arenaMenu.transform.Find("ModalWindow") : mazeMenu?.transform;
        if (menu != null)
        {
            var button = MakeButton(menu, "Settings", "SETTINGS", Open);
            if (arenaMenu != null)
            {
                var label = button.GetComponentInChildren<Text>();
                label.fontSize = label.resizeTextMaxSize = 48;
                var version = MakeText(menu, "Release Version", "v" + Application.version, 24);
                version.alignment = TextAnchor.MiddleRight;
                Place(version.transform, new Vector2(.72f,.02f), new Vector2(.95f,.07f));
            }
            Place(button.transform, arenaMenu != null ? new Vector2(.08f,.17f) : new Vector2(.36f,.10f),
                arenaMenu != null ? new Vector2(.38f,.26f) : new Vector2(.60f,.19f));
        }
        foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            LocalizedLabel.AttachTree(canvas.transform);
        BuildPanel(); BuildMusic(); Refresh();
    }
    private void Update()
    {
        if (PortfolioSettings.IsOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) Close();
    }
    private void BuildPanel()
    {
        var root = new GameObject("Settings Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        root.transform.SetParent(transform);
        var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 500;
        var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280,720); scaler.matchWidthOrHeight = .5f;
        panel = Box(root.transform, "Settings Panel", new Color(.035f,.045f,.055f,1f));
        panel.AddComponent<SafeSettingsArea>();
        var heading = MakeText(panel.transform, "Settings Title", "SETTINGS", 36);
        Place(heading.transform,new Vector2(.12f,.83f),new Vector2(.88f,.94f));
        var language = MakeText(panel.transform,"Language Label","LANGUAGE",24);
        Place(language.transform,new Vector2(.12f,.69f),new Vector2(.40f,.78f));
        chineseButton=MakeButton(panel.transform,"Chinese","简体中文",()=>PortfolioSettings.SetLanguage(true));
        englishButton=MakeButton(panel.transform,"English","English",()=>PortfolioSettings.SetLanguage(false));
        Place(chineseButton.transform,new Vector2(.43f,.69f),new Vector2(.63f,.78f));
        Place(englishButton.transform,new Vector2(.66f,.69f),new Vector2(.86f,.78f));
        var soundLabel=MakeText(panel.transform,"Sound Label","SOUND ENABLED",24);
        Place(soundLabel.transform,new Vector2(.12f,.56f),new Vector2(.60f,.65f));
        var toggleRoot=Box(panel.transform,"Sound Toggle",new Color(.20f,.25f,.28f));
        var toggleRect=toggleRoot.GetComponent<RectTransform>();toggleRect.anchorMin=toggleRect.anchorMax=new Vector2(.80f,.605f);toggleRect.sizeDelta=new Vector2(40,40);toggleRect.anchoredPosition=Vector2.zero;
        var check=Box(toggleRoot.transform,"Check",new Color(.2f,.85f,.65f));
        Place(check.transform,new Vector2(.15f,.15f),new Vector2(.85f,.85f));
        sound=toggleRoot.AddComponent<Toggle>(); sound.targetGraphic=toggleRoot.GetComponent<Image>(); sound.graphic=check.GetComponent<Image>();
        sound.onValueChanged.AddListener(PortfolioSettings.SetSound);
        VolumeRow("Master","MASTER VOLUME",.44f,PortfolioSettings.Master);
        VolumeRow("Music","MUSIC VOLUME",.32f,PortfolioSettings.Music);
        VolumeRow("Effects","EFFECTS VOLUME",.20f,PortfolioSettings.Effects);
        var back=MakeButton(panel.transform,"Close Settings","BACK",Close);
        Place(back.transform,new Vector2(.12f,.055f),new Vector2(.36f,.145f));
        panel.SetActive(false);
    }
    private void VolumeRow(string key,string title,float y,float value)
    {
        var text=MakeText(panel.transform,key+" Label",title,24);
        Place(text.transform,new Vector2(.12f,y),new Vector2(.40f,y+.08f));
        var root=new GameObject(key+" Slider",typeof(RectTransform),typeof(Slider)); root.transform.SetParent(panel.transform,false);
        Place(root.transform,new Vector2(.43f,y),new Vector2(.77f,y+.08f));
        var track=Box(root.transform,"Track",new Color(.2f,.26f,.3f)); Place(track.transform,new Vector2(0,.38f),new Vector2(1,.62f));
        var fill=Box(track.transform,"Fill",new Color(.2f,.8f,.65f));
        var handleArea = new GameObject("Handle Area", typeof(RectTransform));
        handleArea.transform.SetParent(root.transform, false);
        var area = (RectTransform)handleArea.transform;
        area.anchorMin = new Vector2(0,.5f); area.anchorMax = new Vector2(1,.5f);
        area.sizeDelta = new Vector2(-22,32);
        var handle=Box(handleArea.transform,"Handle",Color.white); var hr=handle.GetComponent<RectTransform>();hr.sizeDelta=new Vector2(22,0);
        var slider=root.GetComponent<Slider>(); slider.fillRect=fill.GetComponent<RectTransform>(); slider.handleRect=hr; slider.targetGraphic=handle.GetComponent<Image>();
        slider.minValue=0; slider.maxValue=1; slider.SetValueWithoutNotify(value);
        var number=MakeText(panel.transform,key+" Value",Mathf.RoundToInt(value*100)+"%",22);
        Place(number.transform,new Vector2(.79f,y),new Vector2(.90f,y+.08f));
        slider.onValueChanged.AddListener(v=>{PortfolioSettings.SetVolume(key,v);number.text=Mathf.RoundToInt(v*100)+"%";});
    }
    public void Open()
    {
        if (panel == null || PortfolioSettings.IsOpen) return;
        previousTimeScale=Time.timeScale; Time.timeScale=0; PortfolioSettings.IsOpen=true; panel.SetActive(true); Refresh();
    }
    public void Close()
    {
        if (!PortfolioSettings.IsOpen) return;
        PortfolioSettings.IsOpen=false; panel.SetActive(false); Time.timeScale=previousTimeScale; PortfolioSettings.Save();
    }
    private void Refresh()
    {
        if (music != null) music.volume=PortfolioSettings.Music;
        if (sound != null) sound.SetIsOnWithoutNotify(PortfolioSettings.SoundEnabled);
        if (chineseButton != null) chineseButton.GetComponent<Image>().color=PortfolioSettings.Chinese ? new Color(.12f,.46f,.36f) : new Color(.18f,.23f,.28f);
        if (englishButton != null) englishButton.GetComponent<Image>().color=!PortfolioSettings.Chinese ? new Color(.12f,.46f,.36f) : new Color(.18f,.23f,.28f);
    }
    private void BuildMusic()
    {
        const int rate=22050, seconds=8;
        var samples=new float[rate*seconds];
        for(var i=0;i<samples.Length;i++)
        {
            var t=i/(float)rate; var envelope=Mathf.Pow(Mathf.Sin(Mathf.PI*t/seconds),2);
            samples[i]=envelope*(Mathf.Sin(2*Mathf.PI*110*t)+.5f*Mathf.Sin(2*Mathf.PI*165*t)+.25f*Mathf.Sin(2*Mathf.PI*220*t))*.045f;
        }
        musicClip=AudioClip.Create("Ambient Loop",samples.Length,1,rate,false);musicClip.SetData(samples,0);
        music=gameObject.AddComponent<AudioSource>();music.clip=musicClip;music.loop=true;music.spatialBlend=0;music.volume=PortfolioSettings.Music;music.Play();
    }
    private void OnApplicationPause(bool paused) { if(paused) PortfolioSettings.Save(); }
    private void OnApplicationQuit() => PortfolioSettings.Save();
    private void OnDestroy()
    {
        PortfolioSettings.Changed-=Refresh;PortfolioSettings.IsOpen=false;
        if(Instance==this)Instance=null;if(musicClip!=null)Destroy(musicClip);
    }
    public static GameObject Box(Transform parent,string name,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);go.GetComponent<Image>().color=color;
        Place(go.transform,Vector2.zero,Vector2.one);return go;
    }
    public static Text MakeText(Transform parent,string name,string value,int size)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);
        var text=go.GetComponent<Text>();text.font=LocalizedLabel.Font;text.text=value;text.fontSize=size;text.color=new Color(.94f,.97f,.96f);
        text.alignment=TextAnchor.MiddleLeft;text.resizeTextForBestFit=true;text.resizeTextMinSize=16;text.resizeTextMaxSize=size;text.raycastTarget=false;
        LocalizedLabel.Attach(go);return text;
    }
    public static Button MakeButton(Transform parent,string name,string label,UnityEngine.Events.UnityAction action)
    {
        var go=Box(parent,name,new Color(.16f,.27f,.3f));var button=go.AddComponent<Button>();button.targetGraphic=go.GetComponent<Image>();button.onClick.AddListener(action);
        var text=MakeText(go.transform,"Label",label,25);text.alignment=TextAnchor.MiddleCenter;Place(text.transform,new Vector2(.04f,.06f),new Vector2(.96f,.94f));return button;
    }
    public static void Place(Transform transform,Vector2 min,Vector2 max)
    {
        var rect=(RectTransform)transform;rect.anchorMin=min;rect.anchorMax=max;rect.offsetMin=rect.offsetMax=Vector2.zero;
    }
}

public sealed class SafeSettingsArea : MonoBehaviour
{
    private Rect last;
    private void Update()
    {
        var safe=Screen.safeArea;if(last==safe)return;last=safe;
        PortfolioSettingsMenu.Place(transform,new Vector2(safe.x/Screen.width,safe.y/Screen.height),new Vector2(safe.xMax/Screen.width,safe.yMax/Screen.height));
    }
}
