﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MLAgents
{
    /// CoreBrain which decides actions using developer-provided Decision script.
    public class CoreBrainHeuristic : ScriptableObject, CoreBrain
    {
        [SerializeField] private bool broadcast = true;

        /**< Reference to the brain that uses this CoreBrainHeuristic */
        public Brain brain;

        Batcher brainBatcher;

        /**< Reference to the Decision component used to decide the actions */
        public Decision decision;

        /// Create the reference to the brain
        public void SetBrain(Brain b)
        {
            brain = b;
        }

        /// Create the reference to decision
        public void InitializeCoreBrain(Batcher brainBatcher)
        {
            decision = brain.gameObject.GetComponent<Decision>();

            if ((brainBatcher == null)
                || (!broadcast))
            {
                this.brainBatcher = null;
            }
            else
            {
                this.brainBatcher = brainBatcher;
                ;
                this.brainBatcher.SubscribeBrain(brain.gameObject.name);
            }
        }

        /// Uses the Decision Component to decide that action to take
        public void DecideAction(Dictionary<Agent, AgentInfo> agentInfo)
        {
            brainBatcher?.SendBrainInfo(brain.gameObject.name, agentInfo);

            if (decision == null)
            {
                throw new UnityAgentsException(
                    "The Brain is set to Heuristic, but no decision script attached to it");
            }

            foreach (Agent agent in agentInfo.Keys)
            {
                agent.UpdateVectorAction(decision.Decide(
                    agentInfo[agent].stackedVectorObservation,
                    agentInfo[agent].visualObservations,
                    agentInfo[agent].reward,
                    agentInfo[agent].done,
                    agentInfo[agent].memories));

            }

            foreach (Agent agent in agentInfo.Keys)
            {
                agent.UpdateMemoriesAction(decision.MakeMemory(
                    agentInfo[agent].stackedVectorObservation,
                    agentInfo[agent].visualObservations,
                    agentInfo[agent].reward,
                    agentInfo[agent].done,
                    agentInfo[agent].memories));
            }
        }

        /// Displays an error if no decision component is attached to the brain
        public void OnInspector()
        {
#if UNITY_EDITOR
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            broadcast = EditorGUILayout.Toggle(new GUIContent("Broadcast",
                "If checked, the brain will broadcast states and actions to Python."), broadcast);
            if (brain.gameObject.GetComponent<Decision>() == null)
            {
                EditorGUILayout.HelpBox("You need to add a 'Decision' component to this gameObject",
                    MessageType.Error);
            }
#endif
        }

    }
}
