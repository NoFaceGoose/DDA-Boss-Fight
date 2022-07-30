using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Player player;

    public Animator animator;

    public float moveSpeed;
    public float horizontalMove;

    public bool jump;

    // Update is called once per frame
    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * moveSpeed;

        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
            animator.SetBool("IsJumping", true);
            FindObjectOfType<AudioManager>().Play("Jump");
        }
    }

    public void OnLanding()
    {
        animator.SetBool("IsJumping", false);
    }

    void FixedUpdate()
    {
        // Move our character
        player.Move(horizontalMove * Time.fixedDeltaTime, jump);
        jump = false;
    }
}