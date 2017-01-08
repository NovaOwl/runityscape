﻿using Scripts.Model;
using Scripts.Model.Stats.Attributes;
using Scripts.Model.Stats.Resources;
using Scripts.Presenter;
using Scripts.View.Effects;
using Scripts.View.ObjectPool;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.View.Portraits {

    /// <summary>
    /// This class represents a Portrait viewable
    /// on either the left or right side of the screen.
    /// </summary>
    public class PortraitView : PooledBehaviour {
        public CharacterPresenter Presenter;

        [SerializeField]
        private BuffView buffPrefab;

        [SerializeField]
        private GameObject buffsHolder;

        [SerializeField]
        private GameObject effectsHolder;

        [SerializeField]
        private Image iconImage;

        [SerializeField]
        private Text portraitName;

        //Name of the character
        [SerializeField]
        private ResourceView resourcePrefab;

        //Image of the character
        [SerializeField]
        private GameObject resourcesHolder;

        public IDictionary<AttributeType, AttributeBundle> AttributeViews { get; private set; }

        public GameObject BuffsHolder { get { return buffsHolder; } }

        /// <summary>
        /// Dictionary of all the buff boxes.
        /// </summary>
        public IDictionary<int, BuffBundle> BuffViews { get; private set; }

        /// <summary>
        /// Dictionary of the effects affecting this portrait.
        /// </summary>
        public IDictionary<string, CharacterEffect> Effects { get; private set; }

        public GameObject EffectsHolder { get { return effectsHolder; } }

        public Image Image {
            get {
                return iconImage;
            }
            set {
                iconImage = value;
            }
        }

        public string PortraitName { get { return portraitName.text; } set { portraitName.text = value; } }
        public Text PortraitText { get { return portraitName; } }
        public IDictionary<ResourceType, ResourceBundle> ResourceViews { get; private set; }
        public Sprite Sprite { get { return iconImage.sprite; } set { iconImage.sprite = value; } }

        public void AddEffect(CharacterEffect ce) {
            CharacterEffect current;
            Effects.TryGetValue(ce.ID, out current);
            if (current != null && !current.IsDone) {
                current.StopCoroutine();
                current.CancelEffect();
            }
            ce.StartCoroutine();
            Effects[ce.ID] = ce;
        }

        public void ClearEffects() {
            foreach (KeyValuePair<string, CharacterEffect> pair in Effects) {
                pair.Value.StopCoroutine();
                pair.Value.CancelEffect();
            }
        }

        public override void Reset() {
            PortraitName = "";
            PortraitText.color = Color.white;
            Image.color = Color.white;
            Image.enabled = true;

            ResourceView[] rvs = resourcesHolder.GetComponentsInChildren<ResourceView>();
            BuffView[] bvs = buffsHolder.GetComponentsInChildren<BuffView>();
            PooledBehaviour[] ces = effectsHolder.GetComponentsInChildren<PooledBehaviour>();

            for (int i = 0; i < rvs.Length; i++) {
                ObjectPoolManager.Instance.Return(rvs[i]);
            }

            for (int i = 0; i < bvs.Length; i++) {
                ObjectPoolManager.Instance.Return(bvs[i]);
            }

            for (int i = 0; i < ces.Length; i++) {
                ObjectPoolManager.Instance.Return(ces[i]);
            }

            ResourceViews.Clear();
            BuffViews.Clear();
        }

        public void SetAttributeViews(AttributeType attribute, AttributeBundle bundle) {
            AttributeViews[attribute] = bundle;
        }

        public void SetBuffs(BuffParams[] buffParams) {
            //Set all existing isSets to false.
            List<int> keys = new List<int>(BuffViews.Keys); //Can't modify Dictionary in foreach loop
            foreach (int key in keys) {
                BuffViews[key] = new BuffBundle { buffView = BuffViews[key].buffView, isSet = false };
            }

            //Add or possibly replace new BuffViews.
            foreach (BuffParams b in buffParams) {
                BuffView bv;
                if (!BuffViews.ContainsKey(b.id)) {
                    bv = ObjectPoolManager.Instance.Get(buffPrefab);
                    Util.Parent(bv.gameObject, BuffsHolder);
                } else {
                    bv = BuffViews[b.id].buffView;
                }

                bv.Tooltip = string.Format("{0} - {1}", b.name, b.description);
                bv.Text = b.abbreviation;
                bv.Color = b.color;
                bv.Duration = b.duration;
                BuffViews[b.id] = new BuffBundle { buffView = bv, isSet = true };
            }

            //Check if any isSets are false, if so, remove them and Destroy their gameObjects.
            //We can use same keys list as before since newly added keys cannot be false
            foreach (int key in keys) {
                if (!BuffViews[key].isSet) {
                    ObjectPoolManager.Instance.Return(BuffViews[key].buffView);
                    BuffViews.Remove(key);
                }
            }
        }

        public void SetResources(ResourceType[] resourceTypes) {
            //Set all existing isSets to false.
            List<ResourceType> keys = new List<ResourceType>(ResourceViews.Keys); //Can't modify Dictionary in foreach loop
            foreach (ResourceType key in keys) {
                ResourceViews[key] = new ResourceBundle { resourceView = ResourceViews[key].resourceView, isSet = false };
            }

            //Add or possibly replace new ResourceViews.
            foreach (ResourceType resourceType in resourceTypes) {
                ResourceView rv;
                if (!ResourceViews.ContainsKey(resourceType)) {
                    // Instantiate new resource prefab
                    rv = ObjectPoolManager.Instance.Get(resourcePrefab);
                    Util.Parent(rv.gameObject, resourcesHolder); // Placed in back.

                    // Set resource prefab's details
                    rv.Tooltip = string.Format("{0} - {1}", resourceType.Name, resourceType.Description);
                    rv.Type = resourceType;
                    rv.ResourceName = resourceType.ShortName;
                    rv.ResourceColor = resourceType.FillColor;
                    rv.FillColor = resourceType.FillColor;
                    rv.EmptyColor = resourceType.EmptyColor;

                    // Move resource prefab to appropraite location, resources are ordered.
                    ResourceView[] rvs = resourcesHolder.GetComponentsInChildren<ResourceView>();
                    int index = rvs.Length - 1;

                    // BubbleSort. We use this to move ResourceViews to their appropriate positions. (For example, LIFE is always the topmost resourceview)
                    while (index - 1 >= 0 && rv.Type.CompareTo(rvs[index - 1].Type) < 0) {
                        Util.Swap(rvs[index].gameObject, rvs[index - 1].gameObject);
                        index--;
                    }
                } else {
                    rv = ResourceViews[resourceType].resourceView;
                }

                ResourceViews[resourceType] = new ResourceBundle { resourceView = rv, isSet = true };
            }

            //Check if any isSets are false, if so, remove them and Destroy their gameObjects.
            //We can use same keys list as before since newly added keys cannot be false
            foreach (ResourceType key in keys) {
                if (!ResourceViews[key].isSet) {
                    if (ResourceViews[key].resourceView != null) {
                        ObjectPoolManager.Instance.Return(ResourceViews[key].resourceView);
                    }
                    ResourceViews.Remove(key);
                }
            }
        }

        // Use this for initialization
        private void Awake() {
            ObjectPoolManager.Instance.Register(buffPrefab, 10);
            ObjectPoolManager.Instance.Register(resourcePrefab, 4);

            ResourceViews = new SortedDictionary<ResourceType, ResourceBundle>();
            AttributeViews = new SortedDictionary<AttributeType, AttributeBundle>();
            BuffViews = new SortedDictionary<int, BuffBundle>();
            Effects = new Dictionary<string, CharacterEffect>();
        }

        public struct AttributeBundle {
            public int falseValue;
            public int trueValue;
        }

        public struct BuffBundle {
            public BuffView buffView;
            public bool isSet;
        }

        public struct BuffParams {
            public string abbreviation;
            public Color color;
            public string description;
            public string duration;
            public int id;
            public string name;
        }

        public struct ResourceBundle {
            public bool isSet;
            public ResourceView resourceView;
        }
    }
}