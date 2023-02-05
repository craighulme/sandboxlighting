using Sandbox.Effects;
// using Sandbox;
// using System.Numerics;

namespace EOT.Lighting;
public class TarkovLighting {
  private ScreenEffects postProcess;
  private DepthOfField depthOfField;

  public TarkovLighting() {
    postProcess = Camera.Main.FindOrCreateHook < ScreenEffects > ();
    postProcess.Sharpen = 0.3f;
    postProcess.FilmGrain.Intensity = 0.035f;
    postProcess.FilmGrain.Response = 1;
    postProcess.Vignette.Intensity = 0.02f;
    postProcess.Vignette.Roundness = 0.2f;
    postProcess.Vignette.Smoothness = 0.9f;
    postProcess.Vignette.Color = Color.Black;
    postProcess.Saturation = 1;
    postProcess.MotionBlur.Scale = 0;

    depthOfField = Camera.Main.FindOrCreateHook < DepthOfField > ();
      
  }

  public float GetLightIntensity() {
    var lightEntities = Entity.All.Where(x => x.Tags.Has("light"));

    float totalLightIntensity = 0;
    int numberOfLights = 0;

    foreach(var light in lightEntities) {
      if (light is PointLightEntity pointLight) {
        if (pointLight.Enabled) {
          float lightIntensity = pointLight.Brightness * pointLight.BrightnessMultiplier;
          float lightRange = pointLight.Range;
          Vector3 lightPosition = pointLight.Position;
          Vector3 playerPosition = Game.LocalPawn.Position;

          float distance = Vector3.DistanceBetween(lightPosition, playerPosition);
          if (distance < lightRange) {
            totalLightIntensity += lightIntensity / (1 + distance * distance / (lightRange * lightRange));

            numberOfLights++;
          }
        }
      } else if (light is SpotLightEntity spotLight) {
        if (spotLight.Enabled) {
          float lightIntensity = spotLight.Brightness * spotLight.BrightnessMultiplier;
          float lightRange = spotLight.Range;
          Vector3 lightPosition = spotLight.Position;
          Vector3 playerPosition = Game.LocalPawn.Position;

          float distance = Vector3.DistanceBetween(lightPosition, playerPosition);
          if (distance < lightRange) {
            totalLightIntensity += lightIntensity / (1 + distance * distance / (lightRange * lightRange));

            numberOfLights++;
          }
        }
      } else if (light is EnvironmentLightEntity environmentLight) {
        if (environmentLight.Enabled) {
          float lightIntensity = environmentLight.Brightness;
          float lightRange = float.MaxValue;
          Vector3 lightPosition = environmentLight.Position;
          Vector3 playerPosition = Game.LocalPawn.Position;

          float distance = Vector3.DistanceBetween(lightPosition, playerPosition);
          if (distance < lightRange) {
            // totalLightIntensity += lightIntensity / (1 + distance / lightRange);
            totalLightIntensity += lightIntensity / (1 + distance * distance / (lightRange * lightRange));

            numberOfLights++;
          }
        }
      }
    }

    if (numberOfLights > 0) {
      float averageLightIntensity = totalLightIntensity / numberOfLights;
      averageLightIntensity = (float) Math.Round(averageLightIntensity, 2);
      AdjustEffects(averageLightIntensity);
      return averageLightIntensity;
    } else {
      // Player is not in range of any lights
      AdjustEffects(0.0f);
      return 0.0f;
    }
  }

  private void AdjustEffects(float lightIntensity) {
    Log.Info("Light Intensity: " + lightIntensity);
    // Increase the depth of field effect in bright areas
    if (lightIntensity > 1.2f) {
      postProcess.Saturation = 1.0f;
      depthOfField.Enabled = true;
      depthOfField.FocalDistance = MathX.Lerp(10, 50, lightIntensity);
      depthOfField.BlurSize = MathX.Lerp(1.0f, 2.8f, lightIntensity);
    } else{
      depthOfField.Enabled = false;
    }

    // Increase the vignette effect and saturation in low light areas
    if (lightIntensity < 0.65f) {
      // postProcess.Vignette.enabled = true;
      postProcess.Vignette.Intensity = MathX.Lerp(0.45f, 1.0f, lightIntensity);
      postProcess.Saturation = MathX.Lerp(0.15f, 1.0f, lightIntensity);

      // Enable the screen space ambient occlusion effect
      // postProcess.ambientOcclusion.enabled = true;

      // Enable film grain effect
      // postProcess.FilmGrain.enabled = true;
      postProcess.FilmGrain.Intensity = MathX.Lerp(0.05f, 0.1f, lightIntensity);
    } else {
      // postProcess.Vignette.Enabled = false;
      postProcess.Saturation = 1.0f;
      // postProcess.ambientOcclusion.enabled = false;
      postProcess.FilmGrain.Intensity = 0f;
    }
  }

  public void SetDepthOfField(float focusDistance, float blurSize) {
    depthOfField.Enabled = true;
    depthOfField.FocalDistance = focusDistance;
    depthOfField.BlurSize = blurSize;
  }

  
}
