using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Ghost[] ghosts;
    public Pacman pacman;
    public Transform pellets;

    public int ghostMultiplier { get; private set; } = 1;
    public int score { get; private set; }
    public int lives { get; private set; }

    private new AudioSource audio = null;

    public AudioClip gameStartSFX;
    public AudioClip pelletSFX1;
    public AudioClip pelletSFX2;
    public AudioClip powerPelletSFX;
    public AudioClip sirenSFX1;
    public AudioClip ghostEatenSFX;
    public AudioClip retreatSFX;
    public AudioClip deathSFX;
 
    public bool isFirstPelletEaten;

    private void Awake()
    {
        this.audio = this.gameObject.GetComponent<AudioSource>();
    }
    private void Start()
    { 
        NewGame();
    }

    private void Update()
    {
        if (this.lives <= 0 && Input.anyKeyDown)
        {
            NewGame();
        }
    }

    private void NewGame()
    {
        SetScore(0);
        SetLives(3);
        StartCoroutine(PlayStartSound());
        NewRound();
    }

    private IEnumerator PlayStartSound()
    {
        Time.timeScale = 0;
        this.audio.PlayOneShot(this.gameStartSFX);
        yield return new WaitWhile(() => this.audio.isPlaying);
        Time.timeScale = 1;
        this.audio.clip = this.sirenSFX1;
        this.audio.Play();
    }

    private void NewRound()
    {
        foreach (Transform pellet in this.pellets)
        {
            pellet.gameObject.SetActive(true);
        }

        ResetState();
    }

    private void ResetState()
    {
        ResetGhostMultiplier();

        for (int i = 0; i < this.ghosts.Length; i++)
        {
            this.ghosts[i].ResetState();
        }

        this.pacman.ResetState();

        this.isFirstPelletEaten = false;
}

    private void GameOver()
    {
        for (int i = 0; i < this.ghosts.Length; i++)
        {
            this.ghosts[i].gameObject.SetActive(false);
        }

        this.pacman.gameObject.SetActive(false);
    }

    private void SetScore(int score)
    {
        this.score = score;
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
    }

    public void GhostEaten(Ghost ghost)
    {
        int points = ghost.points * this.ghostMultiplier;
        SetScore(this.score + points);
        this.ghostMultiplier++;
        this.audio.PlayOneShot(this.ghostEatenSFX);
        this.audio.clip = this.retreatSFX;
        this.audio.Play();
    }

    public void PacmanEaten()
    {
        this.pacman.DeathSequence();
        this.audio.Stop();
        float animationTime = deathSFX.length / this.pacman.deathSequence.GetComponent<AnimatedSprite>().sprites.Length;
        this.pacman.deathSequence.GetComponent<AnimatedSprite>().animationTime = animationTime;
        StartCoroutine(PlayDeathSFX());

        SetLives(this.lives - 1);

        if (this.lives > 0)
        {
            Invoke(nameof(ResetState), 3.0f);
        }
        else 
        {
            GameOver();
        }
    }

    private IEnumerator PlayDeathSFX()
    {
        this.pacman.movement.SetDirection(Vector3.up, true);
        this.pacman.transform.rotation = Quaternion.identity;

        this.audio.PlayOneShot(this.deathSFX);
        while (this.audio.isPlaying)
        {
            yield return null;
        }
    }

    public void PelletEaten(Pellet pellet)
    {
        pellet.gameObject.SetActive(false);
        if (!this.isFirstPelletEaten)
        {
            this.audio.PlayOneShot(this.pelletSFX1);
            Invoke(nameof(ResetPelletSFX), 0.25f);
        }
        else
        {
            this.audio.PlayOneShot(this.pelletSFX2);
            CancelInvoke();
        }

        this.isFirstPelletEaten = !this.isFirstPelletEaten;

        SetScore(this.score + pellet.points);

        if (!HasRemainingPellets())
        {
            this.pacman.gameObject.SetActive(false);
            Invoke(nameof(NewRound), 3.0f);
        }
    }

    private void ResetPelletSFX()
    {
        this.isFirstPelletEaten = false;  
    }

    public void PowerPelletEaten(PowerPellet pellet)
    {
        for (int i = 0; i < this.ghosts.Length; i++)
        {
            this.ghosts[i].frightened.Enable(pellet.duration);
        }

        PelletEaten(pellet);
        this.audio.PlayOneShot(powerPelletSFX);
        CancelInvoke();
        Invoke(nameof(ResetGhostMultiplier), pellet.duration);
    }

    private bool HasRemainingPellets()
    {
        foreach (Transform pellet in this.pellets)
        {
            if (pellet.gameObject.activeSelf)
            {
                return true;
            }
        }

        return false;
    }

    private void ResetGhostMultiplier()
    {
        this.ghostMultiplier = 1;
    }
}
