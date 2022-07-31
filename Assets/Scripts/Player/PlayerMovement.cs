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
        horizontalMove = Input.GetAxisRaw("Horizontal") * moveSpeed;

        animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

        // Move our character
        player.Move(horizontalMove * Time.fixedDeltaTime, jump);
        jump = false;
    }
}