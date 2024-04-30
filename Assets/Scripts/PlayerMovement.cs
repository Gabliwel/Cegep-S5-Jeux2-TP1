using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float speed = 5f;
    [SerializeField] private Transform mainCamera;

    private Vector3 direction;
    private float rotationTime = 0.1f;
    private float rotationSpeed;
    
    private float gravity = 30f;
    private float jumpSpeed = 12f;
    private float vecticalMovement = 0f;
    private bool doubleJump = false;

    void Update()
    {
        BuildSurfaceMovement();
        //BuildVerticalMovement();

        characterController.Move(direction);
        Debug.Log(direction);
    }

    private void BuildSurfaceMovement()
    {
        //Get axis donne au déplacement un effet d'ajout progressif au déplacement, comme si on utilisait un joystick analogique
        //GetAxisRaw enlève cet effet.
        //Par contre il est présent naturellement pour un joystick; Avec un Joystick, GetAxisRaw serait préférable.
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        //.normalized fait en sorte que nous aurons un vecteur de 1 de longeur, peu importe la direction
        //Empêche donc le playerController d'aller plus vite en diagonale.  Donc pratique pour le clavier/souris
        //
        //Par contre avec un gamepad cela fait en sorte qu'on a plus l'effet de vitesse progressive quand on enfonce le gamepad à moitié
        //La vitesse est totale ou elle ne l'est pas.
        //Cette ligne de code ne serait donc pas recommandée pour un gamepad.
        direction = new Vector3(horizontal, 0f, vertical).normalized;
        //Debug.Log(direction);

        //Pour éliminer les effets de shake du joystick
        //Nomalement le input manager fait la job, mais parfois, on veut un second niveau de protection
        //Avec le clavier et/ou avec normalized, la magnitude sera toujours de 1.
        if (direction.magnitude >= 0.2f)
        {
            //l'angle que l'on vise avec nos contrôles.  Je pense que ça doit vous dire quelque chose.
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y; ;

            //SmoothDampAngle permet de faire un déplacement progressif entre l'angle actuel et l'angle visé.
            //Sans cette ligne de code, le pivot du personnage sera brutal.
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSpeed, rotationTime);

            //Quaternion.Euler permet de gérer correctement les rotations en degrés malgré que l'on ai affaire à un quaternion.
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            //Si vous voulez que le personnage diminue de vitesse en saut.
            //Si on voudrait que le saut ne change pas de direction: garder le même vecteur en x et z quand le joueur n'est pas grounded
            //Si on veut que la vitesse reste optimal tant qu'on en change pas de direction: enregistrer la direction au saut et ralentir seulement si
            //Cette direction change
            float tempSpeed = speed;
            if (!characterController.isGrounded) tempSpeed /= 2;

            Vector3 moveDirection = (Quaternion.Euler(0f, angle, 0f) * Vector3.forward).normalized;
            direction.x = moveDirection.x * tempSpeed * Time.deltaTime;
            direction.z = moveDirection.z * tempSpeed * Time.deltaTime;
        }
        else
        {
            direction = Vector3.zero;
        }
    }

    private void BuildVerticalMovement()
    {
        if (characterController.isGrounded) doubleJump = true;

        if (Input.GetButtonDown("Jump"))
        {
            if (characterController.isGrounded)
            {
                vecticalMovement = jumpSpeed;
            }
            else if (doubleJump)
            {
                vecticalMovement = jumpSpeed;
                doubleJump = false;
            }
        }

        vecticalMovement -= gravity * Time.deltaTime;
        direction.y = vecticalMovement * Time.deltaTime;
    }
}
