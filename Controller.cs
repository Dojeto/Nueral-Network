using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    public GameObject player;
    private int topDistance;
    private bool isTraning = false;
    private int populationSize = 20;
    private int generationNumber = 0;
    public Text generationText;
    private int[] layers = new int[] { 6, 10, 10, 1 }; //1 input and 1 output
    private List<NeuralNetwork> nets;
    private List<Bird> playerList = null;
    public int amntLeft = 0;
    public Transform[] pipes;
    public float timer;
    public bool runEffectiveLearning = false;

    void Timer()
    {
        if (timer <= 0)
        {
            isTraning = false;
        }
        else
        {
            Invoke("Timer", 120);
        }
    }

    public void RunThisAgain()
    {
        //Debug.Log("test");
        isTraning = false;
    }

    void Update()
    {
        generationText.text = "Generation : " + generationNumber.ToString();
        if (isTraning == false)
        {
            amntLeft = populationSize;

            if (generationNumber == 0)
            {
                InitEntityNeuralNetworks();
            }
            else
            {
                nets.Sort();
                GameObject
                    .Find("Window_Graph")
                    .GetComponent<WindowGraph>()
                    .valueList.Add(nets[populationSize - 1].fitness);
                GameObject.Find("Window_Graph").GetComponent<WindowGraph>().NewEntry();
                if (!runEffectiveLearning)
                {
                    //nets[0].topFitness = true;
                    for (int i = populationSize / 2; i < populationSize - 1; i++)
                    {
                        //nets[i] = new NeuralNetwork(nets[i + (populationSize / 2)]);
                        nets[i] = nets[populationSize - 1];
                        //nets[i].Mutate();
                        nets[i - populationSize / 2].Mutate();

                        //nets[i + (populationSize / 2)] = new NeuralNetwork(nets[i + (populationSize / 2)]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
                    }
                    //nets[0] = nets[populationSize - 1];
                }

                if (runEffectiveLearning)
                {
                    for (int i = 0; i < (populationSize - 2) / 2; i++) //Gathers all but best 2 nets
                    {
                        nets[i] = new NeuralNetwork(nets[i + (populationSize - 2) / 2]); //Copies weight values from top half networks to worst half
                        nets[i].Mutate(); //Mutates new entities

                        nets[i + (populationSize - 2) / 2] = new NeuralNetwork(
                            nets[populationSize - 1]
                        );
                        nets[i + (populationSize - 2) / 2].Mutate();

                        nets[populationSize - 1] = new NeuralNetwork(nets[populationSize - 1]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
                        nets[populationSize - 2] = new NeuralNetwork(nets[populationSize - 2]); //too lazy to write a reset neuron matrix values method....so just going to make a deepcopy lol
                    }
                }

                for (int i = 0; i < populationSize; i++)
                {
                    nets[i].SetFitness(0f);
                }
            }

            generationNumber++;
            topDistance = 0;
            isTraning = true;
            Invoke("Timer", 600);
            timer = 600;
            CreateEntityBodies();
            pipes[0].position = new Vector3(2f, pipes[0].position.y, 0);
            pipes[1].position = new Vector3(7f, pipes[1].position.y, 0);
            pipes[2].position = new Vector3(12f, pipes[2].position.y, 0);
            pipes[3].position = new Vector3(17f, pipes[3].position.y, 0);
            pipes[4].position = new Vector3(22f, pipes[4].position.y, 0);

            amntLeft = populationSize;
            foreach (Bird emt in playerList)
            {
                if (emt.failed)
                {
                    amntLeft--;
                }
            }

            if (amntLeft <= 0)
            {
                isTraning = false;
                amntLeft = populationSize;
                timer = 600;
            }
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
        }

        void CreateEntityBodies()
        {
            if (playerList != null)
            {
                for (int i = 0; i < playerList.Count; i++)
                {
                    GameObject.Destroy(playerList[i].gameObject);
                }
            }

            playerList = new List<Bird>();

            for (int i = 0; i < populationSize; i++)
            {
                Bird birdScript = (
                    (GameObject)Instantiate(
                        player,
                        new Vector3(0f, 0f, 0),
                        player.transform.rotation
                    )
                ).GetComponent<Bird>();
                birdScript.Init(nets[i]);
                birdScript.gameObject.name = "Bird " + i;
                playerList.Add(birdScript);
            }
        }

        void InitEntityNeuralNetworks()
        {
            //population must be even, just setting it to 20 incase it's not
            if (populationSize % 2 != 0)
            {
                populationSize = populationSize - 1;
            }

            nets = new List<NeuralNetwork>();

            for (int i = 0; i < populationSize; i++)
            {
                NeuralNetwork net = new NeuralNetwork(layers);
                net.Mutate();
                nets.Add(net);
            }
        }
    }
}
