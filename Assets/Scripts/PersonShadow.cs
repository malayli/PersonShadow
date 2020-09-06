// Digital Fox

using UnityEngine;

public enum ShadowType { isCastedOnFloor, isStuckToPlayer, isNone };
public enum ShadowState { isGrounded, isNotGrounded, willBeGrounded, WillBeNotGrounded };

public class PersonShadow : MonoBehaviour {
    // Public

    public int sortingOrder = 0;
    public int terrainLayerIndex = 10;
    public float zOrder = 0;
    public Vector2 shadowSize;
    public Vector2 positionDelta;
    public GameObject personGameObject;
    public ShadowType shadowType = ShadowType.isCastedOnFloor;

    // Private
    
    private bool isGrounded = false;
    private float ratio = 1.0f;
    private Vector2 minimumShadowSize;
    private SpriteRenderer[] spriteRenderers;
    private ShadowState shadowState = ShadowState.isNotGrounded;

    // Start is called before the first frame update
    private void Start() {
        LoadSpriteRenderers();
        GetFirstSpriteRenderer().sortingOrder = sortingOrder;
        Reset();
    }

    // Update is called once per frame
    private void FixedUpdate() {
        UpdatePosition();
    }

    private void UpdatePosition() {
        switch (shadowType) {
            case ShadowType.isStuckToPlayer:
                if (personGameObject) {
                    Vector2 personPosition = personGameObject.transform.position;
                    transform.position = new Vector3 (personPosition.x + positionDelta.x, personPosition.y + positionDelta.y, zOrder);
                }
                break;

            case ShadowType.isCastedOnFloor:
                CastShadowOnFloor();
                break;

            case ShadowType.isNone:
                break;
        }
    }

    private void CastShadowOnFloor() {
        if (!personGameObject) {
            return;
        }

        Vector2 personPosition = personGameObject.transform.position;

        // Bit shift the index of the terrain layer to get a bit mask
        int layerMask = (1 << terrainLayerIndex);

        // Casting Shadow to the Floor

        RaycastHit2D hit = Physics2D.Raycast(personPosition, Vector3.down, Mathf.Infinity, layerMask);

        float posY = -10000f;

        if (hit && hit.collider) {
            posY = hit.point.y;
        }

        transform.position = new Vector3 (personPosition.x + positionDelta.x, posY + positionDelta.y, -0.08f);
    }
    
    private void Reset() {
        ratio = 1.0f;
        minimumShadowSize =  new Vector2 (0.241f, 0.204f);
        transform.localScale = shadowSize;
        UpdatePosition();
    }

    // Public API

    public void ResetToGrounded(Vector2 aShadowSize) {
        shadowSize = aShadowSize;
        Reset();
        isGrounded = true;
        shadowState = ShadowState.isGrounded;
        UpdateShadow(true);
    }

    public void UpdateShadow(bool grounded) {
        switch (shadowState) {
            case ShadowState.isGrounded:
                if  (grounded != isGrounded) {
                    isGrounded = grounded;
                    shadowState = ShadowState.WillBeNotGrounded;
                } else {
                    transform.localScale = shadowSize;
                }
                break;

            case ShadowState.WillBeNotGrounded:
                if (ratio > 0.5f) {
                    ratio -= Time.deltaTime*8.0f;
                    transform.localScale = new Vector2 (transform.localScale.x/1.1f, transform.localScale.y/1.1f);

                } else {
                    minimumShadowSize = transform.localScale;
                    shadowState = ShadowState.isNotGrounded;
                }
                break;

            case ShadowState.isNotGrounded:
                if  (grounded != isGrounded) {
                    isGrounded = grounded;
                    shadowState = ShadowState.willBeGrounded;

                } else {
                    ratio = 0.5f;
                    transform.localScale = minimumShadowSize;
                }
                break;

            case ShadowState.willBeGrounded:
                if (ratio < 1.0f) {
                    ratio += Time.deltaTime*8.0f;
                    transform.localScale = new Vector2 (transform.localScale.x*1.1f, transform.localScale.y*1.1f);

                } else {
                    shadowState = ShadowState.isGrounded;
                }
                break;

            default:
                break;
        }
    }
    
    // Sprite Renderers Methods

    private void LoadSpriteRenderers() {
        if (spriteRenderers == null) {
            spriteRenderers = this.GetComponentsInChildren<SpriteRenderer>();
        }

        if (spriteRenderers == null) {
            spriteRenderers = this.GetComponents<SpriteRenderer>();
        }
    }

    private SpriteRenderer GetFirstSpriteRenderer() {
        LoadSpriteRenderers();

        if (spriteRenderers == null) {
            return null;
        }

        if (spriteRenderers.Length == 0) {
            return null;
        }

        return spriteRenderers[0];
    }

    public void ShowSpriteRenderer() {
        GetFirstSpriteRenderer().enabled = true;
    }

    public void HideSpriteRenderer() {
        GetFirstSpriteRenderer().enabled = false;
    }
}
