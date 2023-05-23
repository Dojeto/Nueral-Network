using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    private bool initialized = false;
    private NeuralNetwork net;
    private Rigidbody2D rBody;
    public float timeElapsed;
    public bool failed = false;
    public Transform sensorA,
        sensorB,
        sensorC;
    public float howFarAwayA,
        howFarAwayB;

    public float nextPipeHeight;
    public float nextPipeDistance;

    public LayerMask dangerLayer;
    public LayerMask pipeLayer;
    Vector3 dirA,
        dirB,
        dirC;

    void Start()
    {
        rBody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        dirA = (this.transform.position - sensorA.position).normalized;
        dirB = (this.transform.position - sensorB.position).normalized;
        dirC = (this.transform.position - sensorC.position).normalized;
        if (
            Physics2D.Raycast(
                transform.position,
                transform.position + dirA,
                Mathf.Infinity,
                dangerLayer
            )
        )
        {
            howFarAwayA = Vector2.Distance(
                transform.position,
                Physics2D
                    .Raycast(
                        transform.position,
                        transform.position + dirA,
                        Mathf.Infinity,
                        dangerLayer
                    )
                    .point
            );
        }

        if (
            Physics2D.Raycast(
                transform.position,
                transform.position + dirB,
                Mathf.Infinity,
                dangerLayer
            )
        )
        {
            howFarAwayB = Vector2.Distance(
                transform.position,
                Physics2D
                    .Raycast(
                        transform.position,
                        transform.position + dirB,
                        Mathf.Infinity,
                        dangerLayer
                    )
                    .point
            );
        }

        if (
            Physics2D
                .Raycast(transform.position, transform.position - dirC, Mathf.Infinity, pipeLayer)
                .collider
        )
        {
            nextPipeHeight = Physics2D
                .Raycast(transform.position, transform.position - dirC, Mathf.Infinity, pipeLayer)
                .collider.gameObject.transform.parent.transform.position.y;
            nextPipeDistance = Vector2.Distance(
                transform.position,
                Physics2D
                    .Raycast(
                        transform.position,
                        transform.position - dirC,
                        Mathf.Infinity,
                        pipeLayer
                    )
                    .point
            );
        }
        else
        {
            nextPipeHeight = 0;
        }

        if (!failed)
        {
            timeElapsed += Time.deltaTime;
            float[] inputs = new float[6];

            inputs[0] = nextPipeDistance;
            inputs[1] = (1.6f + nextPipeHeight) - transform.position.y - 0.28f;
            inputs[2] = (-1.6f + nextPipeHeight) - transform.position.y + 0.28f;
            inputs[3] = GetComponent<Rigidbody2D>().velocity.y;
            inputs[4] = howFarAwayA;
            inputs[5] = howFarAwayB;

            float[] output = net.FeedForward(inputs);

            GetComponent<Rigidbody2D>().velocity = Vector2.up * output[0] * 10;

            net.SetFitness(
                (
                    timeElapsed
                    - Vector2.Distance(
                        transform.position,
                        Physics2D
                            .Raycast(
                                transform.position,
                                transform.position - dirC,
                                Mathf.Infinity,
                                pipeLayer
                            )
                            .point
                    )
                ) + 10
            );
        }
        else
        {
            GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.1f);
        }
        //Debug.DrawLine(transform.position, transform.position - dirC, Color.red);

        //Debug.Log(howFarAwayA);
    }

    public void Init(NeuralNetwork net)
    {
        this.net = net;
        initialized = true;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Danger")
        {
            failed = true;
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + dirA * 10);
        Gizmos.DrawLine(transform.position, transform.position + dirB * 10);
        Gizmos.DrawLine(transform.position, transform.position - dirC * 10);
    }
}
