using UnityEngine;
using System.Collections;


public enum Wheels { FrontRight, FrontLeft, RearRight, RearLeft }

public class GameState : MonoBehaviour 
{
    public class Car
    {
        public bool IsGrounded
        {
            get
            {
                return (frwGrounded && flwGrounded && rrwGrounded && rlwGrounded);
            }
        }

        private bool frwGrounded;
        private bool flwGrounded;
        private bool rrwGrounded;
        private bool rlwGrounded;

        public void ToggleWheelFlag(Wheels wheel)
        {
            switch (wheel)
            {
                case Wheels.FrontLeft:
                    flwGrounded = !flwGrounded;
                    break;
                case Wheels.FrontRight:
                    frwGrounded = !frwGrounded;
                    break;
                case Wheels.RearLeft:
                    rlwGrounded = !rlwGrounded;
                    break;
                case Wheels.RearRight:
                    rrwGrounded = !rrwGrounded;
                    break;
            }
        }

        public void ResetWheelFlags()
        {
            frwGrounded = false;
            flwGrounded = false;
            rrwGrounded = false;
            rlwGrounded = false;
        }
    }

	//Declare shared properties.
	private static GameState instance;

	public static GameState Instance
	{
		get
		{
			//create the instance as a gameobject if it doesnt exist.
			if(instance == null)
			{
				instance = new GameObject("GameState").AddComponent<GameState>();
			}

			return instance;
		}
	}

    public Car car;

	//Remove the instance on application quit.
	public void OnApplicationQuit()
	{
		instance = null;
	}

	//Start a new state.
	public void StartState()
	{
		Debug.Log ("[GameState]: Create new State");

        car = new Car();
        car.ResetWheelFlags ();

		//load the first level.
		Application.LoadLevel ("cg_lvl_test");
	}
}
